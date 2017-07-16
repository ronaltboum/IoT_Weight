using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace hx711onUWP
{
    class Hx711 : IHx711
    {
        private GpioController gpio;
        private long offset;
        private float scale;
        private GpioPin dout;
        private GpioPin slk;
        private const byte AVERAGE_DEF = 25;
        private const float SCALE_DEF = 1992f;

        public Hx711(byte pin_dout, byte pin_slk)
        {
            gpio = GpioController.GetDefault();
            dout = gpio.OpenPin(pin_dout);
            slk = gpio.OpenPin(pin_slk);
            dout.SetDriveMode(GpioPinDriveMode.Output);
            slk.SetDriveMode(GpioPinDriveMode.Input);

            slk.Write(GpioPinValue.High);
            System.Threading.Tasks.Task.Delay(1);
            slk.Write(GpioPinValue.Low);

            this.AverageValue();
            this.SetOffset(AverageValue());
            this.SetScale();
        }
        public long AverageValue(byte times)
        {
            long sum = 0;
            for(byte i = 0; i< times; i++)
            {
                sum += GetValue();
            }
            return sum / times;
        }

        public long AverageValue()
        {
            return AverageValue(AVERAGE_DEF);
        }

        public float GetGram()
        {
            long val = (AverageValue() - this.offset);
            return (float)val / this.scale;
        }

        public long GetValue()
        {
            byte[] data = new byte[3];
            GpioPinValue pinValue;
            bool boolPinValue;
            for (byte j=0;j < 3; j++)
            {
                for (byte i = 0; i < 8; i++)
                {
                    slk.Write(GpioPinValue.High);
                    pinValue = dout.Read();
                    boolPinValue = (pinValue == GpioPinValue.High) ? true : false;
                    bitWrite(data[2 - j], 7 - i, boolPinValue);
                    slk.Write(GpioPinValue.Low);
                }
            }

            slk.Write(GpioPinValue.High);
            slk.Write(GpioPinValue.Low);

            return ((long)data[2] << 16) | ((long)data[1] << 8) | (long)data[0];
        }

        public void SetOffset(long offset)
        {
            this.offset = offset;
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void SetScale()
        {
            SetScale(SCALE_DEF);
        }

        /* adaptation of Arduino's bitWrite() methon to RBPi.
         * taken from: https://stackoverflow.com/questions/23339587/similar-function-arduino-bitwrite
         */
        private void bitWrite(ulong x, int n, bool b)
        {
            if (n <= 7 && n >= 0)
            {
                if (b)
                {
                    x |= (1ul << n);
                }
                else
                {
                    x &= ~(1ul << n);
                }
            }
        }
    }
}
