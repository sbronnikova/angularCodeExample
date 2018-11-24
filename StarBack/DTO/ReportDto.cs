namespace Star.Web.Api.DTO
{
    /// <summary>
    /// DTO отчета.
    /// </summary>
    public class ReportDto
    {
        /// <summary>
        /// Группа отчета.
        /// </summary>
        public ReportGroupDto Group { get; set; }
        /// <summary>
        /// Идентификатор сущности.
        /// </summary>
        public string UID { get; set; }
        /// <summary>
        /// Наименование отчета.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Вызов с объекта.
        /// </summary>
        public bool CallFromObject { get; set; }
        /// <summary>
        /// Вызов из главного меню.
        /// </summary>
        public bool CallFromMenu { get; set; }
        /// <summary>
        /// Предаварительный просмотр при генерации.
        /// </summary>
        public bool PreviewOnGeneration { get; set; }
        /// <summary>
        /// Иконка.
        /// </summary>
        public int? IconId { get; set; }
        /// <summary>
        /// Отчет без обязательных параметров, для которых не задано значение по-умолчанию.
        /// </summary>
        public bool HasNoRequiredAndDefaultParams { get; set; }

        /// <summary>
        /// Признак наличия параметров у отчета.
        /// </summary>
        public bool HasParameters { get; internal set; }

        /// <summary>
        /// Идентификатор сущности с параметрами очета.
        /// </summary>
        public string FakeReportEntityUid { get; internal set; }

        /// <summary>
        /// Идентификатор типа сущности с параметрами отчета.
        /// </summary>
        public int FakeEntityTypeId { get; internal set; }
    }
}