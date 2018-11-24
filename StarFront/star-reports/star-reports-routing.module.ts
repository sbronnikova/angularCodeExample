import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { ReportDocumentResolver }   from './services/report-document-resolver-service';
import { ReportTemplateResolver }   from './services/report-template-resolver';

@NgModule({
    imports: [
        RouterModule.forChild([])
    ],
    exports: [
        RouterModule
    ],
    providers: [ ReportDocumentResolver, ReportTemplateResolver ]
})
export class StarReportsRoutingModule { }
