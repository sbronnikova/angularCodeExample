import { Component, Output, OnInit, EventEmitter } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import CustomStore from 'devextreme/data/custom_store';
import Dx from 'devextreme/bundles/dx.all';

import { DxRemoteDataResult } from '../../../star-components/interfaces/dx/dx-remote-data-result';

import { ReportCommunicationService } from '../../services/report-communication.service';
import { ReportsService } from '../../services/reports.service';
import { PageResultReportDocumentDto, ReportDocumentDto, ReportDto, ReportsApi } from '../../../webapi';
import { AnnouncementService } from '../../../star-common/services/announcement.service';
import {AlertService} from '../../../star-common/services/alert.service';
import {AlertOptions} from '../../../star-common/models/alert-options';
import {AlertButtons} from '../../../star-common/models/alert-buttons.enum';

@Component({
    selector: 'report-documents',
    templateUrl: 'report-documents.component.html',
    styleUrls:['report-documents.component.scss']
})
export class ReportDocumentsComponent implements OnInit {
    @Output() error: EventEmitter<string> = new EventEmitter<string>();
    reportTemplate: ReportDto;
    dataSource: any = {};

    private indexMap: Map<string, number> = new Map<string, number>();
    private _selected: ReportDocumentDto[];
    set selected(values: ReportDocumentDto[]){
        this._selected = values;
    }
    get selected(){
        return this._selected;
    }

    constructor(
        private route: ActivatedRoute,
        private reportService: ReportsService,
        private reportCommunicationService: ReportCommunicationService,
        private reportsApi: ReportsApi,
        private announcementService: AnnouncementService,
        private alertService: AlertService
    ) {}

    ngOnInit(): void {
        this.route.data.subscribe((data: { report: ReportDto }) => {
            this.reportTemplate = data.report;
            this.refresh();
        });
    }

    documentPreviewDisabled(): boolean {
        return !(this.selected && this.selected.length > 0);
    }

    refresh() {
        this.selected = [];
        this.dataSource = {
            store: new CustomStore({
                load: loadOptions => this.dataSourceOnLoad(loadOptions)             
            })
        };
    }

    private dataSourceOnLoad(loadOptions: Dx.data.LoadOptions): Promise<DxRemoteDataResult | void> {
        return this.reportsApi.reportsGet_1(this.reportTemplate.uid, loadOptions.skip, loadOptions.take,
             JSON.stringify(loadOptions.sort),JSON.stringify(loadOptions.filter))
            .toPromise()
            .then(apiResult => {
                let pageResult = <PageResultReportDocumentDto>apiResult;
                this.indexMap.clear();
                let skip = loadOptions.skip || 0;
                pageResult.items.forEach((item, index) => this.indexMap.set(item.uid, skip + index));
                let result: DxRemoteDataResult = {
                    data: pageResult.items,
                    totalCount: pageResult.totalCount,
                };
                return result;
            })
            .catch(error => {
                this.announcementService.setError(error);
            });
    }

    previewHtml(documents: ReportDocumentDto[]) {
        if (!documents || documents.length !== 1) {
            return;
        }

        this.reportCommunicationService.changeCurrent({
            uid: documents[0].uid, title: documents[0].name
        });
    }

    previewPdf(documents: ReportDocumentDto[]) {
        if (!documents || documents.length !== 1) {
            return;
        }

        this.reportService.previewDocumentPdf(documents[0]);
    }

    previewXls(documents: ReportDocumentDto[]) {
        if (!documents || documents.length !== 1) {
            return;
        }

        this.reportService.previewDocumentXls(documents[0]);
    }

    generate(): void{
        this.reportsApi.reportsGenerateReport(this.reportTemplate.uid)
        .subscribe(resp => {
            let options = new AlertOptions('Генерация отчета', 'Отчет был успешно добавлен в очередь на генерацию', () => {});
            options.buttons = AlertButtons.Ok;
            this.alertService.openConfirm(options);
        },
        error => {
            this.announcementService.setError(error);
        });
    }

    onError(e: any) {
        this.announcementService.setError(e.error);
    }
}
