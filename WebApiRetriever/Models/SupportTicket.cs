using SqlSugar;

namespace WebApiRetriever.Models
{
    [SplitTable(SplitType.Month)]
    [SugarTable("Support_Tickets_{year}{month}{day}")]
    public class obj_SupportSystem_SupportTicketsSplit
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        [SugarColumn(IsNullable = true)]
        public int GeneralAuditFirmId { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string GeneralAuditFirmName { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string Link { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string GeneralFirmId { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string GeneralFirmName { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string GeneralFirmGUID { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(50)", IsNullable = true)]
        public string YearEnd { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SystemModuleId { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string Title { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string Issue { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string Solution { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string PostedBy { get; set; }

        [SplitField]
        public DateTime PostedDate { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SupportedBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? SupportedDate { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SupportStatus { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SoftwareStatus { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string CompletedBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? CompletedDate { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string SenderUniqueIdentifier { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string SupporterUniqueIdentifier { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string SoftwareUniqueIdentifier { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SolutionBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? SolutionDate { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string EditedBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? EditedDate { get; set; }

        [SugarColumn(IsNullable = true)]
        public int SupportGroup { get; set; }

        [SugarColumn(IsNullable = true)]
        public int SoftwareGroup { get; set; }

        [SugarColumn(IsNullable = true)]
        public int Status { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string Remark { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(100)", IsNullable = true)]
        public string isUrgent { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(100)", IsNullable = true)]
        public string isReopen { get; set; }

        [SugarColumn(IsNullable = true)]
        public int isTransitionData { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(50)", IsNullable = false)]
        public string TicketUID { get; set; }

        public int SupportExternalChatCount { get; set; }

        public int SupportInternalChatCount { get; set; }

        public int SoftwareConversationCount { get; set; }

        public DateTime UpdatedTime { get; set; }

        [SugarColumn(IsNullable = true)]
        public string OriginalPIC { get; set; }

        [SugarColumn(ColumnDataType = "NVARCHAR(MAX)", IsNullable = true)]
        public string Issue2 { get; set; }

        //[SugarColumn(ColumnDataType = "NVARCHAR(10)", IsNullable = true)]
        //public string MappingVersion { get; set; }
        //public string SRC { get; set; }
    }
}
