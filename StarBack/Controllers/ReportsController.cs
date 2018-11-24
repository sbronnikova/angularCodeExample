using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Star.Core.Common;
using Star.Core.DataStruct.EntityCollections;
using Star.ExpressReport;
using Star.Web.Api.DTO;
using Star.Web.Api.Results;
using System.Net.Http;
using DevExpress.XtraPrinting;
using System.Net;
using System.Net.Http.Headers;
using DevExpress.XtraReports.UI;
using System.Web.Http.ModelBinding;
using Star.Web.Api.Exceptions;
using Star.Core.Extensions;
using Star.Core.DataStruct.EntityFilter;
using Star.Core.DataStruct;
using Star.Web.Api.Services;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System.Configuration;
using System.Web.Script.Serialization;
using Star.Core.Helpers;

namespace Star.Web.Api.Controllers
{
    /// <summary>
    /// Работа с отчетами.
    /// </summary>
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        private readonly IReportManager _reportManager;
        private readonly IReportFileService _reportService;
        private readonly IEntityManager _entityManager;
        private readonly IExpressReportCreator _dxReportCreator;
        private readonly IExpressionEvaluator _expressionEvaluator;
        private readonly IExpressionParsing _expressionParsing;
        private readonly FileAccessService<string> _fileAccessService;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="reportManager"></param>
        /// <param name="reportService"></param>
        /// <param name="entityManager"></param>
        /// <param name="dxReportCreator"></param>
        /// <param name="expressionEvaluator"></param>
        /// <param name="expressionParsing"></param>
        /// <param name="fileAccessService"></param>
        public ReportsController(IReportManager reportManager, IReportFileService reportService, IEntityManager entityManager, IExpressReportCreator dxReportCreator,
            IExpressionEvaluator expressionEvaluator, IExpressionParsing expressionParsing, FileAccessService<string> fileAccessService)
        {
            _reportManager = reportManager;
            _reportService = reportService;
            _entityManager = entityManager;
            _dxReportCreator = dxReportCreator;
            _expressionEvaluator = expressionEvaluator;
            _expressionParsing = expressionParsing;
            _fileAccessService = fileAccessService;
        }       

        /// <summary>
        /// Получение шаблона отчета по идентификатору.
        /// </summary>
        /// <param name="reportTemplateIdStr">Идентификатор шаблона отчета.</param>
        /// <returns>Объект шаблона отчета.</returns>
        /// <exception cref="ArgumentException">Пустой или имеющий тип, отличный от Guid, идентификатор шаблона отчета недопустим.</exception>
        // GET: api/reports/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637
        [Authorize]
        [HttpGet]
        [Route("{reportTemplateIdStr}")]
        public ReportDto Get(string reportTemplateIdStr)
        {
            var reportUid = reportTemplateIdStr.AsUniqueIdentifierGuid();
            if (reportUid == null)
                throw new ApiException("Invalid unique identifier.");

            try
            {
                var reportTemplate = _reportManager.GetExpressReport(reportUid);
                string reportUidString = reportTemplate.UID.ToString();
                return new ReportDto()
                {
                    UID = reportUidString,
                    Name = reportTemplate.Name,
                    CallFromMenu = reportTemplate.CallFromMenu,
                    CallFromObject = reportTemplate.CallFromObject,
                    Group = new ReportGroupDto() { UID = reportTemplate.Group, GroupName = reportTemplate.DisplayGroup },
                    PreviewOnGeneration = reportTemplate.PreviewOnGeneration,
                    IconId = reportTemplate.Icon == null ? null : (int?)reportTemplate.Icon.Id,
                    HasNoRequiredAndDefaultParams = reportTemplate.Parameters.All(parameter => !parameter.IsMandatory && parameter.DefaultValue.IsNullOrEmpty()),
                    HasParameters = reportTemplate.Parameters.Any(),
                    FakeReportEntityUid =
                        Helpers.EntityHelper.REPORT_ENTITY_ID_PREFIX + reportUidString,                      
                    FakeEntityTypeId =
                        reportTemplate.FakeReportEntity.EntityType.Id
                };
            }
            catch (ArgumentNullException)
            {
                throw new ApiException(
                    "Report Not Found", HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Получение html разметки отчета.
        /// </summary>
        /// <param name="reportId">Идентификатор отчета</param>
        /// <param name="onlyBody">В ответе придет только тело отчета, не весь html document</param>
        /// <param name="parameters">Строка с параметрами для генерации отчета (Наименование параметра:Выражение для вычисления параметра;).</param>
        /// <param name="entityId">Сущность-контейнер, относительно которой строится отчет.</param>
        /// <returns>Ответ, содержащий html разметку отчета.</returns>
        /// <exception cref="ArgumentException">Пустой или имеющий тип, отличный от Guid, идентификатор шаблона отчета недопустим.</exception>
        // GET: api/reports/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637exportHtml?parameters=c77f78efe_3abe_4793_9633_3fa71363916a:ContainerEntity()&entityId=A215B1D1-7AB1-4CA7-86D7-03E2CD1DD639
        [Route("{reportId}/exportHtml")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage ExportToHtml(string reportId, bool onlyBody= false, string parameters = null, string entityId = null)
        {
            var report = GetXtraReport(reportId, parameters, entityId);

            HtmlExportOptions htmlOptions = report.ExportOptions.Html;

            htmlOptions.InlineCss = true;
            htmlOptions.PageBorderWidth = 0;
            htmlOptions.CharacterSet = "UTF-8";
            htmlOptions.TableLayout = true;
            htmlOptions.RemoveSecondarySymbols = false;
            htmlOptions.Title = "Test Title";
            htmlOptions.EmbedImagesInHTML = true;
            htmlOptions.ExportMode = HtmlExportMode.SingleFile;

            string reportHtml = null;
            using (var memoryStream = new MemoryStream())
            {
                report.ExportToHtml(memoryStream);
                memoryStream.Position = 0;
                using (StreamReader sr = new StreamReader(memoryStream))
                {
                    reportHtml = sr.ReadToEnd();
                }
            }

            if (onlyBody)
            {
                int bodyBegin = reportHtml.IndexOf("<body");
                int bodyEnd = reportHtml.IndexOf(">", bodyBegin);
                reportHtml = reportHtml.Substring(bodyEnd + 1);
                reportHtml = reportHtml.Substring(0, reportHtml.IndexOf("</body>"));
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(reportHtml, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }

        /// <summary>
        /// Короткоживущий одноразовый идентификатор доступа для скачивания сгенерированного файла.
        /// </summary>
        /// <param name="reportId">Идентификатор шаблона отчета, экспорт которого был запущен.</param>
        /// <returns>идентификатор доступа</returns>
        [Authorize]
        [HttpGet]
        [Route("{reportId}/access")]
        public string GetAccessId(string reportId)
        {
            IUniqueIdentifier reportUid = reportId.AsUniqueIdentifierGuid();
            if (reportUid == null)
                throw new ApiException(
                    "Invalid unique identifier.");

            ExpressReport.ExpressReport expressReport = null;
            try
            {
                expressReport = _reportManager.GetExpressReport(reportUid);
            }
            catch (ArgumentNullException)
            {
                throw new ApiException(
                    "Report not found", HttpStatusCode.NotFound);
            }

            return _fileAccessService.GetAccessId(reportId);
        }

        /// <summary>
        /// Получение отчета в формате PDF.
        /// </summary>
        /// <param name="reportId">Идентификатор отчета</param>
        /// <param name="accessId">Одноразовый идентификатор доступа.</param>
        /// <param name="parameters">Строка с параметрами для генерации отчета (Наименование параметра:Выражение для вычисления параметра;).</param>
        /// <param name="entityId">Сущность-контейнер, относительно которой строится отчет.</param>
        /// <returns>Ответ, содержащий pdf содержимое отчета.</returns>
        /// <exception cref="ArgumentException">Пустой или имеющий тип, отличный от Guid, идентификатор шаблона отчета недопустим.</exception>
        // GET: api/reports/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637/exportPdf/f8nep-45GpDWMa3iqoZs9A3?parameters=c77f78efe_3abe_4793_9633_3fa71363916a:ContainerEntity()&entityId=A215B1D1-7AB1-4CA7-86D7-03E2CD1DD639
        [Route("{reportId}/exportPdf/{accessId}")]
        [HttpGet]
        public HttpResponseMessage ExportToPdf(string reportId, string accessId, string parameters = null, string entityId = null)
        {
            var report = GetXtraReport(reportId, parameters, entityId, accessId);

            PdfExportOptions exportOptions = report.ExportOptions.Pdf;

            exportOptions.ConvertImagesToJpeg = true;
            exportOptions.ImageQuality = PdfJpegImageQuality.Medium;
            exportOptions.ShowPrintDialogOnOpen = false;

            var memoryStream = new MemoryStream();

            report.ExportToPdf(memoryStream);
            memoryStream.Position = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentLength = memoryStream.Length;
            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("inline")
                {
                    FileName = $"{report.PrintingSystem.ExportOptions.PrintPreview.DefaultFileName}.pdf",
                };

            return response;
        }

        /// <summary>
        /// Получение отчета в формате XLS.
        /// </summary>
        /// <param name="reportId">Идентификатор отчета</param>
        /// <param name="accessId">Одноразовый идентификатор доступа.</param>
        /// <param name="parameters">Строка с параметрами для генерации отчета (Наименование параметра:Выражение для вычисления параметра;).</param>
        /// <param name="entityId">Сущность-контейнер, относительно которой строится отчет.</param>
        /// <returns>Ответ, содержащий XLS содержимое отчета.</returns>
        /// <exception cref="ArgumentException">Пустой или имеющий тип, отличный от Guid, идентификатор шаблона отчета недопустим.</exception>
        // GET: api/reports/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637/exportXls/f8nep-45GpDWMa3iqoZs9A3?parameters=c77f78efe_3abe_4793_9633_3fa71363916a:ContainerEntity()&entityId=A215B1D1-7AB1-4CA7-86D7-03E2CD1DD639
        [Route("{reportId}/exportXls/{accessId}")]
        [HttpGet]
        public HttpResponseMessage ExportToXls(string reportId, string accessId, string parameters = null, string entityId = null)
        {
            var report = GetXtraReport(reportId, parameters, entityId, accessId);

            XlsExportOptions exportOptions = report.ExportOptions.Xls;

            exportOptions.ExportMode = XlsExportMode.SingleFilePageByPage;

            var memoryStream = new MemoryStream();

            report.ExportToXls(memoryStream);
            memoryStream.Position = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xls");
            response.Content.Headers.ContentLength = memoryStream.Length;

            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"{report.PrintingSystem.ExportOptions.PrintPreview.DefaultFileName}.xls",
                };

            return response;
        }      

        /// <summary>
        /// Получение сформированных отчетов по заданному шаблону.
        /// </summary>
        /// <param name="reportTemplateIdStr">Идентификатор шаблона отчета.</param>
        /// <param name="skip">Сколько пропустить записей.</param>
        /// <param name="top">Размер страницы (сколько взять записей).</param>
        /// <param name="sort">Сортировка</param>
        /// <param name="filter">Фильтрация</param>
        /// <returns>Объект страницы заданного размера (или все записи).</returns>
        /// <exception cref="ArgumentException">Пустой или имеющий тип, отличный от Guid, идентификатор шаблона отчета невалиден.</exception>
        // GET: api/reports/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637/documents
        [Route("{reportTemplateIdStr}/documents")]
        [HttpGet]
        [Authorize]
        public PageResult<ReportDocumentDto> Get(string reportTemplateIdStr, int skip = 0, [ModelBinder]int? top = null,
            string sort = null,
            string filter = null)
        {
            var reportUid = reportTemplateIdStr.AsUniqueIdentifierGuid();
            if (reportUid == null)
                throw new ApiException("Invalid unique identifier.");

            bool hasNoRequiredOrVirtualParamters = false;
            try
            {
                var reportTemplate = _reportManager.GetExpressReport(reportUid);
                hasNoRequiredOrVirtualParamters = reportTemplate.Parameters.All(parameter => !parameter.IsMandatory && parameter.DefaultValue.IsNullOrEmpty());
            }
            catch (ArgumentNullException)
            {
                throw new ApiException(
                    "Report Not Found", HttpStatusCode.NotFound);
            }

            int totalCount;


            JavaScriptSerializer jsonScriptSerializer = new JavaScriptSerializer();
            var sortModels = !sort.IsNullOrEmpty() ? jsonScriptSerializer.Deserialize<IEnumerable<DataGridSortViewModel>>(sort) : Enumerable.Empty<DataGridSortViewModel>();
            var filterModels = DxStoreHelper.ParseFilters(!filter.IsNullOrEmpty() ? jsonScriptSerializer.Deserialize<object[]>(filter) : new object[0]).ToArray();

            var result = _reportService.Search(reportUid, new RecordsPaging(skip, top), filterModels, sortModels, out totalCount);

            return new PageResult<ReportDocumentDto>
            {
                Items = result.Select(view => new ReportDocumentDto
                {
                    UID = view.Report.UID.ToString(),
                    Name = view.Report.Attachment.Name,
                    ReportTemplateId = reportTemplateIdStr,
                    AttachmentId = view.Report.AttachmentId,
                    CreatedAt = view.Report.CreatedAt,
                    CreatedBy = new UserDto() { UserName = view.CreatedByUser.UserName, Name = view.CreatedByUser.Name }
                }),
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Генерация и сохранение отчета.
        /// </summary>
        /// <param name="reportTemplateId">Идентификатор шаблона отчета.</param>
        /// <returns>Успешность или нет операции.</returns>
        // POST: api/reports/F215B1D1-7AB1-4CA7-86D7-03E2CD1DD637/generate
        [HttpPost]
        [Authorize]
        [Route("{reportTemplateId}/generate")]
        public void GenerateReport(string reportTemplateId)
        {
            var remoteSchedulerClient = new RemoteSchedulerClient();
            remoteSchedulerClient.StartRemoteJobImmediately("Reports", "ReportGeneration",
                new JobDataMap((IDictionary<string, object>)new Dictionary<string, object>() {
                        { "reportTemplateId", reportTemplateId },
                        { "userName", User.Identity.Name }
                }));
        }

        private XtraReport GetXtraReport(string reportId, string parameters = null, string entityId = null, string accessId = null)
        {
            if (accessId != null)
            {
                string accessedReportId = _fileAccessService.GetAccessObject(reportId, accessId);
                if (accessedReportId.IsNullOrEmpty())
                    throw new ApiException(
                        "Report not found", HttpStatusCode.NotFound);
            }

            // идентификатор отчета доступен -> ищем сам отчет
            IUniqueIdentifier reportUid = reportId.AsUniqueIdentifierGuid();
            if (reportUid == null)
                throw new ApiException(
                    "Invalid unique identifier.");

            var xtraReport = new XtraReport();
            ExpressReport.ExpressReport expressReport = null;
            try
            {
                expressReport = _reportManager.GetExpressReport(reportUid);
            }
            catch (ArgumentNullException)
            {
                throw new ApiException(
                    "Report not found", HttpStatusCode.NotFound);
            }

            IEntity entity = null;
            if (!entityId.IsNullOrEmpty())
            {
                entity = _entityManager.GetEntity(entityId.AsUniqueIdentifierGuid());
            }

            // Получаем параметры:
            var expressParameters = GetParameters(expressReport, parameters, entity);

            // Определяем имя документа
            var documentName = _reportManager.GetReportFileName(expressReport);

            // Получаем имя файла по умолчанию, удалив недопустимые символы:
            var invalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            var defaultFileName = new string(documentName.Where(c => !invalidFileNameChars.Contains(c)).ToArray());

            try
            {
                xtraReport = _dxReportCreator.CreateReport(expressReport, expressParameters, false);
                xtraReport.PrintingSystem.ExportOptions.PrintPreview.DefaultFileName = defaultFileName;

                return xtraReport;
            }
            catch (OutOfMemoryException)
            {

            }

            return xtraReport;
        }

        private Dictionary<ExpressReportParameter, object> GetParameters(ExpressReport.ExpressReport expressReport, string parametersString, IEntity entity)
        {
            var parameters = expressReport.FakeReportEntity.GetParametersValues();

            if (!parametersString.IsNullOrEmpty() && entity != null)
            {
                var paramsPairs = parametersString.Split(';');
                foreach (var paramPair in paramsPairs)
                {
                    var pair = paramPair.Split(':');
                    if (pair.Length == 2)
                    {
                        string paramName = pair[0];
                        string paramValue = pair[1].Replace("<colon>", ":").Replace("<semicolon>", ";");

                        if (paramValue.IsNullOrEmpty())
                            continue;

                        Expression paramExpression = _expressionParsing.Parse(paramValue);

                        if (paramExpression == null || paramExpression == Expression.Empty)
                            continue;

                        string errorMessage;
                        parameters[parameters.FirstOrDefault(p => p.Key.Key == paramName).Key] =
                            _expressionEvaluator.Evaluate(paramExpression, entity, out errorMessage);
                    }
                }
            }

            return parameters;
        }
    }
}