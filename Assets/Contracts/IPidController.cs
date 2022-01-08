using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Contracts
{
    public interface IPidController : IAutomaticController
    {
        void Ready(float kp, float ki, float kd);
    }
}
