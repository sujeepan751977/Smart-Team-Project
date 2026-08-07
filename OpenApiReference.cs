using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Recruitment_Project
{
    internal class OpenApiReference
    {
        public ReferenceType Type { get; internal set; }
        public string ReferenceV3 { get; internal set; }
    }
}