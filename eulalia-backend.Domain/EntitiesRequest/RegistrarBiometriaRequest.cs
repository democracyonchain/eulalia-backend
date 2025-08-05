using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulalia_backend.Domain.EntitiesRequest
{
    public class RegistrarBiometriaRequest
    {
        public string Cedula { get; set; }
        public IFormFile TemplateFile { get; set; }
    }
}
