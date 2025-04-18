using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElderCare.Data.Ingestion.Service.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Service.Models
{
    public class LDR : GenericSensor<double>
    {
        public bool IsActive { get; set; }
        public override double GenerateValue()
        {
            throw new NotImplementedException();
        }

        private double GenerateLuminosity()
        {
            if (!IsActive)
            {
                
            }
        }
    }
}
