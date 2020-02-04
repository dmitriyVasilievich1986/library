using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using ModBus_Library;

namespace Support_Class
{
    public class My_Button_Click
    {
        public ModBus_Libra port;
        public Addres_Controls addres;

        public byte[] data;

        public My_Button_Click(ModBus_Libra using_port, Addres_Controls using_addres, byte[] data_to_send)
        {
            port = using_port;
            addres = using_addres;
            data = data_to_send;
        }

        public void send_data(byte set)
        {
            data[0] = addres.Addres;
            if (data[1] == 0x10) { for (int a = 8; a < data.Length; a += 2) data[a] = set; }
            else data[5] = set;
            port.Interrupt(data);
        }
    }

    public class Addres_Controls
    {
        private byte addres;

        public Addres_Controls(byte Addres) { addres = Addres; }

        public byte Addres
        {
            get { return addres; }
            set { addres = value; }
        }
    }

    public class My_Button_Colorized
    {
        int position;
        short addres;

        ModBus_Libra port;            
        Addres_Controls dout_addres;        

        public My_Button_Colorized(ModBus_Libra using_port, short color_check_color, Addres_Controls dout_addres_color, int position_color)
        {
            port = using_port;
            addres = color_check_color;
            dout_addres = dout_addres_color;
            position = position_color;
        }

        public (Color, bool) Checkout(ModBus_Libra checking_port, Color init_color)
        {
            var output = (init_color, false);
            try
            {
                if (port.Port.PortName == checking_port.Port.PortName && port.Data_Receive[1] == 0x02 &&
                (short)((short)(port.Data_Transmit[2] << 8) | (short)port.Data_Transmit[3]) == addres &&
                port.Data_Receive[0] == dout_addres.Addres)
                {
                    short some = port.Data_Receive[2] == 1 ? (short)port.Data_Receive[3] : (short)((short)(port.Data_Receive[3] << 8) | (short)port.Data_Receive[4]);
                    output.Item1 = (some & position) != 0 ? Color.Red : init_color;
                    output.Item2 = true;
                }
            }
            catch (Exception) { }
            return output;
        }
    }

    public class My_Button_Result
    {
        public ModBus_Libra port;
        public Addres_Controls dout_addres;

        public short addres;
        int position;
        public float result = 0;

        public My_Button_Result(ModBus_Libra checkout_port, Addres_Controls checkout_dout_addres, short checkout_addres, int checkout_position)
        {
            port = checkout_port;
            dout_addres = checkout_dout_addres;
            addres = checkout_addres;
            position = checkout_position;
        }

        public float Checkout(ModBus_Libra port_checkout)
        {
            try
            {
                if (port.Port.PortName != port_checkout.Port.PortName) return result;
                if (port.Data_Receive[0] == dout_addres.Addres && port.Data_Receive[1] == 0x04 &&
                    (short)((short)(port.Data_Transmit[2] << 8) | (short)port.Data_Transmit[3]) == addres)
                { result = port.Result[position]; }
            }
            catch (Exception) { }
            return result;
        }
    }

    public class My_Button : Button
    {
        public string name = "";
        public string text = "";

        Color initial_color;
        public My_Button_Click click;
        public My_Button_Colorized colorized;
        public My_Button_Result button_result;

        public My_Button(string using_text, string using_name, Color using_color, My_Button_Click my_button_click = null, My_Button_Colorized button_color = null, My_Button_Result result = null)
        {
            button_result = result;
            click = my_button_click;
            colorized = button_color;
            if (click != null) { Click += (s, e) => { click.send_data(BackColor == Color.Red ? (byte)0 : (byte)1); }; }
            initial_color = using_color;
            TextAlign = ContentAlignment.MiddleLeft;
            Padding = new Padding(40, 0, 0, 0);
            BackColor = initial_color;
            text = using_text;
            name = using_name;
            Text = text;
            Dock = DockStyle.Top;
            FlatStyle = FlatStyle.Flat;
            Height = 35;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold, GraphicsUnit.Point, 204);
        }

        public void Checkout(ModBus_Libra checking_port)
        {
            if (colorized != null) { if(colorized.Checkout(checking_port, initial_color).Item2) BackColor = colorized.Checkout(checking_port, initial_color).Item1; }
            if (button_result != null) button_result.Checkout(checking_port);
        }

        public float Result
        {
            get { if (button_result != null) return button_result.result; return 0; }
            set { if (button_result != null) Text = button_result.result < .5f ? text + $"{button_result.result:0.000}" : text + $"{button_result.result:0.0}"; }
        }
    }

    public class My_Panel : Panel
    {
        public string name = "";

        public My_Panel(string using_name, Padding padding, List<My_Button> buttons = null)
        {
            name = using_name;
            Padding = padding;
            Dock = DockStyle.Top;
            Width = 450;
            AutoScroll = true;
            if (buttons != null) { foreach (My_Button mb in buttons) Controls.Add(mb); Height = 35 + buttons.Count * 35; }
        }
    }

    public class Module_Button : Button
    {
        public Module_Button(string text, EventHandler event_handler)
        {
            Click += event_handler;
            Text = text;
            BackColor = Color.FromArgb(113, 125, 137);
            Dock = DockStyle.Top;
            FlatAppearance.BorderSize = 0;
            FlatStyle = FlatStyle.Flat;
            Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Padding = new Padding(20, 0, 0, 0);
            Height = 35;
            TextAlign = ContentAlignment.MiddleLeft;
        }
    }

    public class Module_Parameters
    {
        public Addres_Controls v12;
        
        public Addres_Controls module;
        public Module_Setup using_module;
        public Addres_Controls dout_din16;
        public Addres_Controls dout_din32;
        public Addres_Controls current_psc;
        public Addres_Controls dout_control;

        public Module_Parameters(byte module_addres, byte dout_controls_addres, byte dout_din16_addres, byte dout_din32_addres, byte psc, byte mtu5, Module_Setup module_selected)
        {
            using_module = module_selected;
            v12 = new Addres_Controls(mtu5);
            current_psc = new Addres_Controls(psc);
            module = new Addres_Controls(module_addres);
            dout_din16 = new Addres_Controls(dout_din16_addres);
            dout_din32 = new Addres_Controls(dout_din32_addres);
            dout_control = new Addres_Controls(dout_controls_addres);
        }
    }

    public class Min_Max_None
    {
        public int Min;
        public int Max;
        public int None;

        public Min_Max_None(int min, int max, int none = 0)
        {
            Min = min;
            Max = max;
            None = none;
        }
    }

    public class Module_Setup
    {
        public string name = "";
        public int power_chanel = 0;
        public int exchange_chanel = 0;
        public float current = 0;

        public Dictionary<string, byte[]> all_registers;

        public Min_Max_None din;
        public Min_Max_None kf;
        public Min_Max_None tc;
        public Min_Max_None tc12v;

        public Module_Setup(string module_name, int power_chanel_count, int exchange_chanel_count, float module_current, Min_Max_None DIN, Min_Max_None KF, Min_Max_None TC, Min_Max_None TC12V, Dictionary<string, byte[]> using_registers = null)
        {
            current = module_current;
            all_registers = using_registers;
            tc12v = TC12V;
            din = DIN;
            kf = KF;
            tc = TC;
            name = module_name;
            power_chanel = power_chanel_count;
            exchange_chanel = exchange_chanel_count;
        }
    }

    public class Send_Data
    {
        public byte[] data;
        public ModBus_Libra port;
        public Addres_Controls addres;

        public Send_Data(byte[] sending_data, ModBus_Libra using_prot, Addres_Controls using_addres)
        {
            data = sending_data;
            port = using_prot;
            addres = using_addres;
        }

        public void sending()
        {
            data[0] = addres.Addres;
            port.Transmit(data);
        }
    }

}
