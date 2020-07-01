using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.CarScripts
{
    public interface IStreamLogger
    {
        void Log(string serializedFrame);
    }
}
