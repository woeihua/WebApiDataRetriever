using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using WebApiRetriever.Models;
using System.Data;
using ProjectHelperClass;
using System.Collections.Generic;
using System.Text.Json;
using System.Runtime.Serialization;

namespace WebApiRetriever.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataRetrieverController : ControllerBase
    {
        private readonly SqlSugarClient _db;
        private readonly SqlSugarClient _dbVTA;

        public DataRetrieverController(IConfiguration configuration)
        {
            //configuration.GetConnectionString("DefaultConnection")

            _db = new SqlSugarClient(new ConnectionConfig()
            {
                ConfigId = "SupportDB",
                ConnectionString = configuration.GetConnectionString("DefaultConnection"),
                DbType = SqlSugar.DbType.SqlServer,
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings
                {
                    IsWithNoLockQuery = true
                }
            });
            _dbVTA = new SqlSugarClient(new ConnectionConfig()
            {
                ConfigId = "VTATrainingDB",
                ConnectionString = configuration.GetConnectionString("VTAConnection"),
                DbType = SqlSugar.DbType.SqlServer,
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings
                {
                    IsWithNoLockQuery = true
                }
            });
        }

        public static string _ReturnFieldText(string text, bool isSkipDate = false, bool isSkipDec = false)
        {
            return FieldHelper.SQLReturnNull(FieldHelper.SQLStringCheckField(text, isSkipDate, isSkipDec));
        }

        // 1. GetSupportTicketList
        [HttpGet("GetSupportTicketList")]
        public IActionResult GetSupportTicketList(long tid)
        {
            var list = _db.Queryable<obj_SupportSystem_SupportTicketsSplit>()
                .SplitTable()
                .Where(it => it.GeneralAuditFirmId==3 && it.PostedDate>DateTime.Parse("2025-05-01"))
                .ToList();

            return Ok(list);
        }

        // 2. GetVTAProgressList
        [HttpGet("GetVTAProgressList")]
        public IActionResult GetVTAProgressList(string BatchId= "1029")
        {
            //var list = _db.Queryable<VtaProgress>()
            //    .Where(it => it.CreatedDate >= startDate && it.CreatedDate <= endDate)
            //    .Select(it => new { it.UserId, it.ProgressStatus }) // Projecting to match your JSON requirement
            //    .ToList();

            //return Ok(list);

            var batch_result = new obj_VtaBatchCandidateResult();

            string error_message = "";

            try
            {
                string str_sql = @"
                DECLARE @BatchId int=" + _ReturnFieldText(BatchId) + @";
                DECLARE @CandidatePartResult TABLE
                (
                    PartNo varchar(50),
                    LoginId int,
                    ResultMark decimal(18,2),
                    TotalTask int,
                    TotalComplete int,
                    TotalCorrect int,
                    PartStartDate datetime,
                    PartEndDate datetime
                )
                DECLARE @TrainingPackageId int;

                SELECT TOP 1 @TrainingPackageId=[TrainingPackageId] FROM [VTATrainingDB].[dbo].[VTA_Batch] WHERE [Id]=@BatchId;

                INSERT INTO @CandidatePartResult
                  SELECT DISTINCT
                    d.[SubjectNo] AS [AssessmentId]
                   ,a.[LoginId]
                   ,(SUM(b.[ResultMark]) / 100) / COUNT(b.[Id]) * 100 AS [ResultMark]
                   ,ISNULL(COUNT(CASE
                      WHEN c.[HandsOn] = 1 AND
                        c.[CheckReady] = 1 THEN 1
                    END), 0) AS TotalTask
                   ,COUNT(CASE
                      WHEN c.[HandsOn] = 1 AND
                        c.[CheckReady] = 1 AND
                        b.[FileName] IS NOT NULL THEN 1
                    END) AS TotalComplete
                   ,COUNT(CASE
                      WHEN c.[HandsOn] = 1 AND
                        c.[CheckReady] = 1 AND
                        b.[ResultMark] = 100 THEN 1
                    END) TotalCorrect
                   ,a.[PartStartDate] AS PartStartDate
                   ,(SELECT TOP 1
                        [CreatedDate]
                      FROM [VTATrainingDB].[dbo].[VTA_BatchCandidatePartLog] WITH (NOLOCK)
                      WHERE [BatchCandidateId] = a.[Id]
                      AND [LogType] = 'End')
                    AS PartEndDate
                  FROM [VTATrainingDB].[dbo].[VTA_BatchCandidatePart] a WITH (NOLOCK)
                  INNER JOIN [VTATrainingDB].[dbo].[VTA_BatchCandidateResult] b WITH (NOLOCK)
                    ON a.[Id] = b.[BatchCandidateId]
                  INNER JOIN [VTATrainingDB].[dbo].[VTA_SubjectAssesment] c WITH (NOLOCK)
                    ON b.[AssessmentId] = c.[Id]
                  INNER JOIN [VTATrainingDB].[dbo].[VTA_Subject] d WITH (NOLOCK)
                    ON c.[SubjectIdentity] = d.[SubjectIdentity]
                  WHERE a.[BatchId]=@BatchId
                  GROUP BY d.[SubjectNo]
                          ,a.[LoginId]
                          ,a.[PartStartDate]
                          ,a.[Id]
                  ORDER BY a.[LoginId] ASC
                  , d.[SubjectNo] ASC

                --table 1
                SELECT
                  a.[LoginId]
                 ,a.[Name]
                 ,a.[AuditFirmId]
                 ,SUM(b.[TotalCorrect]) AS [TotalCorrect]
                FROM [UserDB].[dbo].[Login] a
                LEFT JOIN @CandidatePartResult b
                  ON a.[LoginId] = b.[LoginId]
                WHERE EXISTS (SELECT
                    *
                  FROM [VTATrainingDB].[dbo].[VTA_BatchCandidatePart] t1 WITH (NOLOCK)
                  WHERE t1.[LoginId] = a.[LoginId]
                  AND t1.[BatchId] = @BatchId)
                GROUP BY a.[LoginId]
                        ,a.[Name]
                        ,a.[AuditFirmId]
                ORDER BY SUM(b.[TotalCorrect]) DESC, a.[LoginId]

                --table 2
                SELECT DISTINCT
                  a.[SubjectNo] AS PartId
                 ,a.[SubjectNo] AS PartNo
                 ,a.[SubjectName] AS PartDisplayName
                FROM [VTATrainingDB].[dbo].[VTA_Subject] a WITH (NOLOCK)
                WHERE [Status] = 1
                AND EXISTS (SELECT
                    1
                  FROM [VTATrainingDB].[dbo].[VTA_Batch] t2 WITH (NOLOCK)
                  WHERE t2.[TrainingPackageId] = a.[TrainingPackageId]
                  AND t2.[Id] = @BatchId)
                GROUP BY a.[SubjectNo]
                        ,a.[SubjectName]

                --table 3
                SELECT
                  [SubjectNo] AS PartId
                 ,[Id] AS SubjectId
                 ,[SubjectIdentity]
                 ,[SubjectTopicName] AS SubjectDisplayName
                 ,[Location]
                 ,[Status]
                 ,[SubjectOrder]
                FROM [VTATrainingDB].[dbo].[VTA_Subject] a WITH (NOLOCK)
                WHERE [Status] = 1 AND a.[TrainingPackageId]=@TrainingPackageId
                --ORDER BY [SubjectNo], [SubjectOrder]

                --table 4
                SELECT
                  CAST(b.[Id] AS VARCHAR) AS AssessmentId
                 ,a.[SubjectNo] AS PartId
                 ,b.[SubjectIdentity]
                 ,b.[AssementName] AS AssessmentDisplayName
                 ,b.[AssementOrder]
                 ,b.[HandsOn]
                 ,b.[CheckReady]
                 ,b.[AssementOrder]
                FROM [VTATrainingDB].[dbo].[VTA_Subject] a WITH (NOLOCK)
                INNER JOIN [VTATrainingDB].[dbo].[VTA_SubjectAssesment] b WITH (NOLOCK)
                  ON a.[SubjectIdentity] = b.[SubjectIdentity]
                WHERE a.[Status] = 1
                AND b.[Status] = 1
                AND a.[TrainingPackageId] = @TrainingPackageId

                --table 5
                SELECT
                  CAST(b.[AssessmentId] AS VARCHAR) AS [AssessmentId]
                 ,a.[LoginId]
                 ,CASE
                    WHEN b.[FileName] IS NULL THEN NULL
                    ELSE ISNULL(b.[ResultMark], 0)
                  END AS [ResultMark]
                 ,c.[HandsOn]
                 ,b.[FileName]
                 ,CASE
                    WHEN b.[CreatedDate] = b.[EditedDate] THEN NULL
                    ELSE b.[EditedDate]
                  END AS LastClickDate
                FROM [VTATrainingDB].[dbo].[VTA_BatchCandidatePart] a WITH (NOLOCK)
                INNER JOIN [VTATrainingDB].[dbo].[VTA_BatchCandidateResult] b WITH (NOLOCK)
                  ON a.[Id] = b.[BatchCandidateId]
                INNER JOIN [VTATrainingDB].[dbo].[VTA_SubjectAssesment] c WITH (NOLOCK)
                  ON b.[AssessmentId] = c.[Id]
                WHERE EXISTS (SELECT
                    1
                  FROM [VTATrainingDB].[dbo].[VTA_Batch] t1 WITH (NOLOCK)
                  WHERE a.[BatchId] = t1.[Id]
                  AND t1.[Id] = @TrainingPackageId)
                --ORDER BY a.[LoginId],
                --b.[AssessmentId]
 
                --table 6
                SELECT *,
                PartNo AS [AssessmentId]
                FROM @CandidatePartResult
                ";

                DataSet ds = new DataSet();

                ds = _dbVTA.Ado.GetDataSetAll(str_sql);
                DataTable dt_candidate = null, dt_part = null, dt_subject = null
                    , dt_assessment = null, dt_score_result = null, dt_part_score = null;

                if (FieldHelper.VerifyDataSet(ds))
                {
                    dt_candidate = ds.Tables[0];
                    if (ds.Tables.Count > 1)
                    {
                        dt_part = ds.Tables[1];
                    }
                    if (ds.Tables.Count > 2)
                    {
                        dt_subject = ds.Tables[2];
                    }
                    if (ds.Tables.Count > 3)
                    {
                        dt_assessment = ds.Tables[3];
                    }
                    if (ds.Tables.Count > 4)
                    {
                        dt_score_result = ds.Tables[4];
                    }
                    if (ds.Tables.Count > 5)
                    {
                        dt_part_score = ds.Tables[5];
                    }
                }

                if (FieldHelper.VerifyDataTable(dt_candidate))
                {
                    foreach (DataRow dr_user in dt_candidate.Rows)
                    {
                        batch_result.CandidateList.Add(new obj_VtaCandidateInfo
                        {
                            LoginId = dr_user["LoginId"].ToString(),
                            Name = dr_user["Name"].ToString(),
                            AuditFirmId = dr_user["AuditFirmId"].ToString()
                        });
                    }
                }

                if (FieldHelper.VerifyDataTable(dt_part))
                {
                    foreach (DataRow dr in dt_part.Rows)
                    {
                        obj_VtaPart part = new obj_VtaPart();

                        part.PartId = dr["PartId"].ToString();
                        part.PartNo = dr["PartNo"].ToString();
                        part.PartSeq = part.PartNo.Substring(1, part.PartNo.Length - 1);
                        part.PartDisplayName = dr["PartDisplayName"].ToString();
                        part.IsExpand = false;
                        part.CandidatePartInfoList = GetCandidatePartResultFilter(part.PartId, batch_result.CandidateList, dt_part_score);

                        DataRow[] dr_list_subject = null;

                        if (FieldHelper.VerifyDataTable(dt_subject))
                        {
                            dr_list_subject = dt_subject.Select("PartId=" + _ReturnFieldText(part.PartId));
                            if (dr_list_subject.Length > 0)
                            {
                                int subject_counter = 1;
                                foreach (DataRow dr_subject in dr_list_subject)
                                {
                                    obj_VtaSubject subject = new obj_VtaSubject();
                                    subject.SubjectId = dr_subject["SubjectId"].ToString();
                                    subject.SubjectDisplayName = dr_subject["SubjectDisplayName"].ToString();
                                    subject.SubjectIdentity = dr_subject["SubjectIdentity"].ToString();
                                    subject.IsExpand = false;

                                    subject.Location = dr_subject["Location"].ToString();
                                    subject.SubjectNo = part.PartSeq + Convert.ToChar(subject_counter + 64);
                                    subject_counter++;

                                    DataRow[] dr_list_assessment = null;
                                    if (FieldHelper.VerifyDataTable(dt_assessment))
                                    {
                                        dr_list_assessment = dt_assessment.Select("SubjectIdentity=" + _ReturnFieldText(subject.SubjectIdentity));

                                        if (dr_list_assessment.Length > 0)
                                        {
                                            int assessment_counter = 1;
                                            foreach (DataRow dr_assessment in dr_list_assessment)
                                            {
                                                obj_VtaAssessment assessment = new obj_VtaAssessment();

                                                assessment.AssessmentId = dr_assessment["AssessmentId"].ToString();
                                                assessment.AssessmentDisplayName = dr_assessment["AssessmentDisplayName"].ToString();
                                                assessment.CandidateScoreList = GetCandidateResultFilter(assessment.AssessmentId, batch_result.CandidateList, dt_score_result);

                                                assessment.HandsOn = dr_assessment["HandsOn"].ToString() == "1";
                                                assessment.CheckReady = dr_assessment["CheckReady"].ToString() == "1";
                                                assessment.AssessmentNo = assessment_counter.ToString();
                                                assessment_counter++;

                                                subject.AssessmentList.Add(assessment);
                                            }
                                        }

                                    }
                                    part.SubjectList.Add(subject);
                                }
                            }
                        }
                        batch_result.PartList.Add(part);
                    }
                }

                for (int i = 0; i < batch_result.CandidateList.Count; i++)
                {
                    int user_total_task = 0, user_total_correct = 0;
                    foreach (obj_VtaPart part in batch_result.PartList)
                    {
                        string part_total = part.CandidatePartInfoList[i].TotalTask;
                        string part_correct = part.CandidatePartInfoList[i].TotalCorrect;
                        user_total_task += int.Parse(part_total);
                        user_total_correct += int.Parse(part_correct);
                    }
                    batch_result.CandidateList[i].TotalTask = string.IsNullOrWhiteSpace(user_total_task.ToString()) ? "0" : user_total_task.ToString();
                    batch_result.CandidateList[i].TotalCorrect = string.IsNullOrWhiteSpace(user_total_correct.ToString()) ? "0" : user_total_correct.ToString();
                }

            }
            catch (Exception ex)
            {
                error_message = ex.Message;
            }

            //var listResult = new List<obj_VtaBatchCandidateResult> { batch_result };
            //var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            //return batch_result;
            //var listResult = JsonSerializer.Serialize(batch_result,options); 
            return Ok(batch_result);
        }

        private List<string> GetCandidateResultFilter(string assessmentId, List<obj_VtaCandidateInfo> candidateList, DataTable dt_score_result)
        {
            List<string> score_list = new List<string>();

            DataRow[] dr_list = null;
            if (FieldHelper.VerifyDataTable(dt_score_result))
            {
                for (int i = 0; i < candidateList.Count; i++)
                {
                    dr_list = dt_score_result.Select("AssessmentId=" + _ReturnFieldText(assessmentId) + " AND LoginId=" + _ReturnFieldText(candidateList[i].LoginId));

                    if (dr_list.Length > 0)
                    {
                        foreach (DataRow dr in dr_list)
                        {
                            score_list.Add(dr["ResultMark"].ToString());
                        }
                    }
                }
            }
            return score_list;
        }

        private List<obj_VtaBatchCandidatePartInfo> GetCandidatePartResultFilter(string assessmentId, List<obj_VtaCandidateInfo> candidateList, DataTable dt_score_result)
        {
            List<obj_VtaBatchCandidatePartInfo> score_list = new List<obj_VtaBatchCandidatePartInfo>();

            DataRow[] dr_list = null;
            if (FieldHelper.VerifyDataTable(dt_score_result))
            {
                for (int i = 0; i < candidateList.Count; i++)
                {
                    dr_list = dt_score_result.Select("AssessmentId=" + _ReturnFieldText(assessmentId) + " AND LoginId=" + _ReturnFieldText(candidateList[i].LoginId));

                    if (dr_list.Length > 0)
                    {
                        foreach (DataRow dr in dr_list)
                        {
                            score_list.Add(new obj_VtaBatchCandidatePartInfo
                            {
                                ResultMark = dr["ResultMark"].ToString(),
                                TotalTask = dr["TotalTask"].ToString(),
                                TotalComplete = dr["TotalComplete"].ToString(),
                                TotalCorrect = dr["TotalCorrect"].ToString(),
                                PartStartDate = dr["PartStartDate"].ToString(),
                                PartEndDate = dr["PartEndDate"].ToString()
                            });
                        }
                    }
                }
            }
            return score_list;
        }

    }
}
