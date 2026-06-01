namespace WebApiRetriever.Models
{
    //[Serializable]
    public class VtaProgress
    {
        public string UserId { get; set; }
        public double ProgressStatus { get; set; }
    }

    //[Serializable]
    public class obj_VtaBatch
    {
        public string BatchId { get; set; }
        public string BatchNo { get; set; }//Id + 10000
        public string TrainingPackage { get; set; }
        public string AuditFirmName { get; set; }
        public string AuditFirmId { get; set; }
        public string BatchName { get; set; }
        public string CandidateNo { get; set; }
        public string CandidateDone { get; set; }
        public string CandidateStatus { get; set; }
        public bool BatchStatus { get; set; }

    }

    public class obj_VtaCandidateInfo
    {
        public string LoginId { get; set; }
        public string Name { get; set; }
        public string AuditFirmId { get; set; }

        public string TotalTask { get; set; }
        public string TotalCorrect { get; set; }
    }

    //[Serializable]
    public class obj_VtaBatchCandidateResult
    {
        public List<obj_VtaCandidateInfo> CandidateList =new List<obj_VtaCandidateInfo>();  

        public List<obj_VtaPart> PartList = new List<obj_VtaPart>();
    }

    //[Serializable]
    public class obj_VtaBatchCandidatePartInfo
    {
        public string ResultMark { get; set; }
        public string TotalTask { get; set; }
        public string TotalComplete { get; set; }
        public string TotalCorrect { get; set; }
        public string PartStartDate { get; set; }
        public string PartEndDate { get; set; }

    }

    //[Serializable]
    public class obj_VtaPart
    {
        public string PartId { get; set; }
        public string PartNo { get; set; }
        public string PartSeq { get; set; }
        public string PartDisplayName { get; set; }
        public string PartStartDate { get; set; }
        public string PartEndDate { get; set; }
        public string CompanyId { get; set; }
        public bool DoneStatus { get; set; }
        public bool CompleteStatus { get; set; }
        public bool IsExpand { get; set; }

        public List<obj_VtaSubject> SubjectList = new List<obj_VtaSubject>();

        public List<string> CandidateScoreList = new List<string>();

        public List<obj_VtaBatchCandidatePartInfo> CandidatePartInfoList = new List<obj_VtaBatchCandidatePartInfo>();

        public List<string> CandidateDoneList = new List<string>();

        public string BatchCandidateId { get; set; }
        public string BatchId { get; set; }

        public string TotalTask { get; set; }
        public string TotalComplete { get; set; }
        public string TotalCorrect { get; set; }

    }

    //[Serializable]
    public class obj_VtaSubject
    {
        public string SubjectId { get; set; }
        public string SubjectDisplayName { get; set; }
        public string SubjectIdentity { get; set; }
        public bool IsExpand { get; set; }
        public string SubjectNo { get; set; }
        public string Location { get; set; }
        public string ExtraInfo { get; set; }

        public List<obj_VtaAssessment> AssessmentList = new List<obj_VtaAssessment>();
    }

    //[Serializable]
    public class obj_VtaAssessment
    {
        public string AssessmentNo { get; set; }
        public string AssessmentId { get; set; }
        public string AssessmentDisplayName { get; set; }
        public bool IsExpand { get; set; }
        public bool HandsOn { get; set; }
        public bool SysReady { get; set; }
        public bool CheckReady { get; set; }

        public List<obj_VtaCellContent> LocationList = new List<obj_VtaCellContent>();
        public List<obj_VtaCellContent> VideoList = new List<obj_VtaCellContent>();
        public List<obj_VtaCellContent> MaterialList = new List<obj_VtaCellContent>();
        public List<obj_VtaCellContent> SystemGuideList = new List<obj_VtaCellContent>();
        public List<obj_VtaCellContent> TeachGuideList = new List<obj_VtaCellContent>();
        public string ResultMark { get; set; }
        public string ResultFilePath { get; set; }
        public string FileName { get; set; }
        public string BatchCandidateResultId { get; set; }


        public List<string> CandidateScoreList = new List<string>();
    }

    //[Serializable]
    public class obj_VtaCellContent
    {
        public string CellId { get; set; }
        public string AssessmentId { get; set; }
        public string Category { get; set; }
        public string CellType { get; set; }
        public string CellName { get; set; }
        public string CellColSpan { get; set; }
        public string CellRowSpan { get; set; }
        public string CellTitle { get; set; }
        public string CellDescription { get; set; }
        public string CellCss { get; set; }
        public string CellUrl { get; set; }
    }
}
