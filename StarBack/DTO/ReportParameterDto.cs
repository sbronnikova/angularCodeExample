using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Star.Web.Api.DTO
{
    /// <summary>
    /// Dto входного параметра отчета.
    /// </summary>
    public class ReportParameterDto
    {
        /// <summary>
        /// Системное имя.
        /// </summary>        
        public string Key { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>        
        public string Name { get; set; }

        /// <summary>
        /// Имя типа параметра.
        /// </summary>        
        public string TypeName { get; set; }

        /// <summary>
        /// Является ли массивом.
        /// </summary>        
        public bool IsArray { get; set; }        

        /// <summary>
        /// Выражение для значения по умолчанию.
        /// </summary>        
        public string DefaultValue { get; set; }
                
        /// <summary>
        /// Является ли параметр вычисляемым.
        /// </summary>        
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Является ли параметр обязательным.
        /// </summary>        
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Вызов с объекта.
        /// </summary>        
        public bool CallFromObject { get; set; }
    }
}