namespace Star.Web.Api.DTO
{
    /// <summary>
    /// DTO группы отчета.
    /// </summary>
    public class ReportGroupDto
    {
        /// <summary>
        /// Идентификатор сущности.
        /// </summary>
        public string UID { get; set; }
        /// <summary>
        /// Наименование группы отчета.
        /// </summary>
        public string GroupName { get; set; }
    }
}