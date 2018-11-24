using System.Web.Http;
using Star.ExpressReport;
using Star.Web.Api.DTO;
using System.Linq;
using System.Collections.Generic;
using Star.Core.Common;
using Star.Web.Api.Exceptions;
using System;

namespace Star.Web.Api.Controllers
{
    /// <summary>
    /// Работа с группами отчетов.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/reportGroups")]
    public class ReportGroupsController : ApiController
    {
        private readonly IReportManager _reportManager;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="reportManager"></param>
        public ReportGroupsController(IReportManager reportManager)
        {
            _reportManager = reportManager;
        }

        /// GET: api/ReportGroup
        /// <summary>
        /// Получение списка групп отчетов.
        /// </summary>
        /// <returns>Массив объектов групп отчетов.</returns>
        [Route("")]
        [HttpGet]
        public IEnumerable<ReportGroupDto> Get()
        {
            return _reportManager.GetGroups()
                .Select(item =>
                    new ReportGroupDto
                    {
                        UID = item.UID.ToString(),
                        GroupName = item.GroupName
                    });
        }


        /// <summary>
        /// Получение списка групп отчетов вместе с отчетами.
        /// </summary>
        /// <returns></returns>
        [Route("withReports")]
        [HttpGet]
        public IEnumerable<ReportGroupWithReportsDto> GetWithReports()
        {
            IEnumerable<ReportGroupWithReportsDto> result = _reportManager
                .GetGroups()
                .Select(g =>
                    new ReportGroupWithReportsDto
                    {
                        UID = g.UID.ToString(),
                        GroupName = g.GroupName,
                        Reports = _reportManager
                            .GetReports(g.UID)
                            .Select(r =>
                                new ReportDto
                                {
                                    UID = r.UID.ToString(),
                                    Name = r.Name,
                                    CallFromMenu = r.CallFromMenu,
                                    CallFromObject = r.CallFromObject,
                                    Group =
                                        new ReportGroupDto()
                                        {
                                            UID = r.GroupUid.ToString(),
                                            GroupName = r.DisplayGroup
                                        },
                                    PreviewOnGeneration = r.PreviewOnGeneration,
                                    IconId = r.Icon?.Id
                                })
                    });      
            return result;
        }

        /// <summary>
        /// Получение списка отчетов заданной группы.
        /// </summary>
        /// <param name="groupIdStr">Идентификатор группы</param>
        /// <returns>Массив объектов шаблонов отчетов.</returns>
        /// <exception cref="ArgumentException">Пустой или имеющий тип, отличный от Guid, идентификатор группы недопустим.</exception>
        // GET: api/reportGroups/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637/reports
        [Authorize]
        [HttpGet]
        [Route("{groupIdStr}/reports")]
        public ReportDto[] GetReports(string groupIdStr)
        {
            IUniqueIdentifier groupUid = groupIdStr.AsUniqueIdentifierGuid();
            if (groupUid == null)
                throw new ApiException(
                    "Invalid unique identifier.");

            return _reportManager.GetReports(groupUid.AsGuid().Value)
                .Select(view => new ReportDto
                {
                    UID = view.UID.ToString(),
                    Name = view.Name,
                    CallFromMenu = view.CallFromMenu,
                    CallFromObject = view.CallFromObject,
                    Group = new ReportGroupDto() { UID = view.GroupUid.ToString(), GroupName = view.DisplayGroup },
                    PreviewOnGeneration = view.PreviewOnGeneration,
                    IconId = view.Icon == null ? null : (int?)view.Icon.Id
                }).ToArray();
        }
    }
}