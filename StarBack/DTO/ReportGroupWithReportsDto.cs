using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Star.Web.Api.DTO
{
    /// <summary>
    /// Группа со списком отчетов.
    /// </summary>
    public class ReportGroupWithReportsDto: ReportGroupDto
    {
        /// <summary>
        /// Отчеты группы.
        /// </summary>
        public IEnumerable<ReportDto> Reports { get; set; }
    }
}