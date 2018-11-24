/**
 * Star.Web.Api
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: v1
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */

import * as models from './models';

/**
 * Модель сгенерированного отчета (с данными).
 */
export interface ReportDocumentDto {
    /**
     * Шаблон отчета, по которому генерировался документ.
     */
    reportTemplateId?: string;

    /**
     * Идентификатор сущности.
     */
    uid?: string;

    /**
     * Наименование отчета.
     */
    name?: string;

    /**
     * Дата генерации отчета.
     */
    createdAt?: Date;

    /**
     * Пользователь, сгенерировавший отчет.
     */
    createdBy?: models.UserDto;

    /**
     * Идентификатор сформированного файла отчета.
     */
    attachmentId?: number;

}