import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ClarityModule } from '@clr/angular';

import { DxDataGridModule } from 'devextreme-angular';

import { StarCommonModule } from '../star-common/star-common.module';
import { StarComponentsModule } from '../star-components/star-components.module';
import { StarReportsRoutingModule } from './star-reports-routing.module';

import { ReportCommunicationService } from './services/report-communication.service';
import { ReportsService } from './services/reports.service';

import { ReportGroupReportTemplatesComponent } from './components/report-group-node/report-group-report-templates.component';
import { ReportTreeComponent } from './components/report-tree/report-tree.component';
import { ReportDocumentsComponent } from './components/reports/report-documents.component';
import { ReportGroupsComponent } from './components/report-groups/report-groups.component';
import { ReportPrintPreviewComponent } from './components/report-print-preview/report-print-preview.component';
import { ReportCardPreviewComponent } from './components/report-card-preview/report-card-preview.component';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ClarityModule.forChild(),
        DxDataGridModule,
        StarCommonModule,
        StarComponentsModule,
        StarReportsRoutingModule
    ],
    declarations: [
        ReportGroupReportTemplatesComponent,
        ReportTreeComponent,
        ReportDocumentsComponent,
        ReportGroupsComponent,
        ReportPrintPreviewComponent,
        ReportCardPreviewComponent
    ],
    providers: [
        ReportsService,
        ReportCommunicationService
    ],
    exports: [ ReportCardPreviewComponent ]
})
export class StarReportsModule { }
