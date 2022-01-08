using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Contracts
{
    public interface IAutomaticController
    {
        float GetOutputValue(float error, float deltaTime); 
    }
}
