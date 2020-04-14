using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using ModBus_Library;
using System.Data.SqlClient;

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

        ModBus_Libra port;
        ModBus_Libra port2;
        public Registers addres;

        public My_Button_Colorized(ModBus_Libra using_port, Registers using_addres, int position_color, ModBus_Libra checkout_port2 = null)
        {
            port = using_port;
            port2 = checkout_port2;
            addres = using_addres;
            position = position_color;
        }

        public (Color, bool) Checkout(ModBus_Libra checking_port, Color init_color)
        {
            var output = (init_color, false);
            try
            {
                if (port.Port.PortName == checking_port.Port.PortName && port.Data_Receive[1] == 0x02 &&
                (short)((short)(port.Data_Transmit[2] << 8) | (short)port.Data_Transmit[3]) == addres.Register_short &&
                port.Data_Receive[0] == addres.addres.Addres)
                {
                    short some = port.Data_Receive[2] == 1 ? (short)port.Data_Receive[3] : (short)((short)(port.Data_Receive[3] << 8) | (short)port.Data_Receive[4]);
                    output.Item1 = (some & position) != 0 ? Color.Red : init_color;
                    output.Item2 = true;
                }
            }
            catch (Exception) { }
            if (port2 != null)
                try
                {
                    if (port2.Port.PortName == checking_port.Port.PortName && port2.Data_Receive[1] == 0x02 &&
                    (short)((short)(port2.Data_Transmit[2] << 8) | (short)port2.Data_Transmit[3]) == addres.Register_short &&
                    port2.Data_Receive[0] == addres.addres.Addres)
                    {
                        short some = port2.Data_Receive[2] == 1 ? (short)port2.Data_Receive[3] : (short)((short)(port2.Data_Receive[3] << 8) | (short)port2.Data_Receive[4]);
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
        public ModBus_Libra port2;
        public Registers addres;

        int position;
        public float result = 0;

        public My_Button_Result(ModBus_Libra checkout_port, Registers checkout_addres, int checkout_position, ModBus_Libra checkout_port2 = null)
        {
            port2 = checkout_port2;
            port = checkout_port;
            addres = checkout_addres;
            position = checkout_position;
        }

        public float Checkout(ModBus_Libra port_checkout)
        {
            if (port.Port.PortName == port_checkout.Port.PortName)
            {
                try
                {
                    if (port_checkout.Data_Receive[0] == addres.addres.Addres && port_checkout.Data_Receive[1] == 0x04 &&
                        (short)((short)(port_checkout.Data_Transmit[2] << 8) | (short)port_checkout.Data_Transmit[3]) == addres.Register_short)
                    { result = port_checkout.Result[position]; }
                }
                catch (Exception) { }
            }
            if (port2 != null)
                if(port2.Port.PortName == port_checkout.Port.PortName)
                {
                    try
                    {
                        if (port_checkout.Data_Receive[0] == addres.addres.Addres && port_checkout.Data_Receive[1] == 0x04 &&
                            (short)((short)(port_checkout.Data_Transmit[2] << 8) | (short)port_checkout.Data_Transmit[3]) == addres.Register_short)
                        { result = port_checkout.Result[position]; }
                    }
                    catch (Exception) { }
                }
            return result;
        }
    }

    public class My_Control_Visible
    {
        public Registers register;

        public byte position;

        public My_Control_Visible(Registers using_register, byte using_position = 1)
        {
            register = using_register;
            position = using_position;
        }
    }

    public class My_Button : Button
    {
        public string name = "";
        public string text = "";

        Color initial_color;
        public My_Button_Click click;
        public My_Control_Visible my_button_visible;
        public My_Button_Colorized colorized;
        public My_Button_Result button_result;

        public My_Button(string using_text, string using_name, Color using_color, Color my_button_font_color, My_Button_Click my_button_click = null, My_Button_Colorized button_color = null, My_Button_Result result = null, My_Control_Visible visible = null)
        {
            my_button_visible = visible;
            ForeColor = my_button_font_color;
            button_result = result;
            click = my_button_click;
            colorized = button_color;
            //if (click != null) { Click += (s, e) => { click.send_data(BackColor == Color.Red ? (byte)0 : (byte)1); }; }
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

        public void set_button()
        {
            if (click != null) click.send_data(BackColor == Color.Red ? (byte)0 : (byte)1);
        }

        public void Reset()
        {
            if (BackColor == Color.Red)  click.send_data(0);
        }

        public void visible(Module_Parameters mp)
        {
            if (my_button_visible == null) return;
            if (mp.using_module.all_registers.Register.Find(x => x.name == my_button_visible.register.name).Register[5] >= my_button_visible.position || mp.using_module.name.ToLower() == "no module") 
            {
                Visible = true;
                if (mp.using_module.name.ToLower() != "no module" && button_result != null)
                {
                    button_result.addres.Register_short = mp.using_module.all_registers.Register.Find(x => x.name == button_result.addres.name).Register_short;
                    button_result.addres.addres = mp.using_module.all_registers.Register.Find(x => x.name == button_result.addres.name).addres;
                }
                if (mp.using_module.name.ToLower() != "no module" && colorized != null && mp.using_module.all_registers.Register.Exists(x => x.name == colorized.addres.name)) 
                {
                    colorized.addres.Register_short = mp.using_module.all_registers.Register.Find(x => x.name == colorized.addres.name).Register_short;
                }
            }
            else Visible = false;
        }

        public void Checkout(ModBus_Libra checking_port)
        {
            if (colorized != null) { if(colorized.Checkout(checking_port, initial_color).Item2) BackColor = colorized.Checkout(checking_port, initial_color).Item1; }
            if (button_result != null) Result = button_result.Checkout(checking_port);
        }

        public float Result
        {
            get { if (button_result != null) return button_result.result > .1f ? (float)Math.Round(button_result.result, 2) : (float)Math.Round(button_result.result, 3); return 0; }
            set { if (button_result != null) Text = button_result.result < 1f ? text + $"{button_result.result:0.000}" : text + $"{button_result.result:0.0}"; }
        }
    }

    public class My_Panel : Panel
    {
        public string name = "";

        Color initial_color;
        public My_Button_Colorized colorized;
        public My_Control_Visible my_panel_visible;

        public My_Panel(string using_name, Padding padding, Color using_color, List<My_Button> buttons = null, My_Control_Visible panel_visible = null, My_Button_Colorized panel_color = null)
        {
            initial_color = using_color;
            colorized = panel_color;
            BackColor = initial_color;
            my_panel_visible = panel_visible;
            name = using_name;
            Padding = padding;
            Dock = DockStyle.Top;
            Width = 450;
            AutoScroll = true;
            if (buttons != null) { foreach (My_Button mb in buttons) Controls.Add(mb); Height = 35 + buttons.Count * 35; }
        }

        public void visible(Module_Parameters mp)
        {
            if (my_panel_visible == null) return;
            if (mp.using_module.all_registers.Register.Find(x => x.name == my_panel_visible.register.name).Register[5] > 0 || mp.using_module.name.ToLower() == "no module") 
            {
                Visible = true;
                width_with_buttons();
            }
            else Visible = false;
        }

        public void width_with_buttons()
        {
            Height = Padding.Vertical;
            foreach (My_Button mb in Controls) if (mb.Visible) Height += 35;
        }

        public void Checkout(ModBus_Libra checking_port)
        {
            if (colorized != null) { if (colorized.Checkout(checking_port, initial_color).Item2) BackColor = colorized.Checkout(checking_port, initial_color).Item1; }
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

        public Module_Parameters(Module_Setup module_name)
        {
            using_module = module_name;
            v12 = new Addres_Controls(0);
            current_psc = new Addres_Controls(0);
            module = new Addres_Controls(0);
            dout_din16 = new Addres_Controls(0);
            dout_din32 = new Addres_Controls(0);
            dout_control = new Addres_Controls(0);
        }

        //public Module_Parameters(byte module_addres, byte dout_controls_addres, byte dout_din16_addres, byte dout_din32_addres, byte psc, byte mtu5, Module_Setup module_selected)
        //{
        //    using_module = module_selected;
        //    v12 = new Addres_Controls(mtu5);
        //    current_psc = new Addres_Controls(psc);
        //    module = new Addres_Controls(module_addres);
        //    dout_din16 = new Addres_Controls(dout_din16_addres);
        //    dout_din32 = new Addres_Controls(dout_din32_addres);
        //    dout_control = new Addres_Controls(dout_controls_addres);
        //}
    }

    public class Min_Max_None
    {
        public float Min;
        public float Max;
        public float None;

        public Min_Max_None(float min = 0, float max = 0, float none = 0)
        {
            Min = min;
            Max = max;
            None = none;
        }

        public void setup(string[] parameters)
        {
            Min = float.Parse(parameters[0]);
            Max = float.Parse(parameters[1]);
            if (parameters.Length >= 3) None = float.Parse(parameters[2]);
            else None = 0;

        }
    }

    public class Registers
    {
        public string name;
        public Addres_Controls addres;
        byte[] register;

        public Registers(string reg_name, Addres_Controls using_addres)
        {
            addres = using_addres;
            name = reg_name;
            register = new byte[6];
        }

        public Registers()
        {
            addres = new Addres_Controls(0);
            name = "";
            register = new byte[6];
        }

        public Registers(string reg_name, Addres_Controls using_addres, byte[] sending_data)
        {
            addres = using_addres;
            name = reg_name;
            register = sending_data;
        }

        public Registers(string reg_name)
        {
            addres = new Addres_Controls(0);
            name = reg_name;
            register = new byte[6];
        }

        public byte[] Register
        {
            get { register[0] = addres.Addres; return register; }
            set { register = value; }
        }

        public short Register_short
        {
            get { return (short)((short)(register[2] << 8) | (short)register[3]); }
            set { register[2] = (byte)(value >> 8); register[3] = (byte)value; }
        }
    }

    public class All_Registers
    {
        public Registers kf = new Registers("kf");
        public Registers tc = new Registers("tc");
        public Registers tu = new Registers("tu");
        public Registers entu = new Registers("entu");
        public Registers power = new Registers("power");
        public Registers din16 = new Registers("din16");
        public Registers din32 = new Registers("din32");
        public Registers mtu5tu = new Registers("mtu5 tu");
        public Registers battary = new Registers("battary");
        public Registers PSCoutu = new Registers("PSC out u");
        public Registers PSCouti = new Registers("PSC out i");
        public Registers battaryi = new Registers("battary i");
        public Registers temperature = new Registers("temperature");

        public List<Registers> Register
        {
            get { return new List<Registers>() { kf, tc, tu, entu, power, battary, din16, din32, mtu5tu, temperature, PSCoutu, PSCouti, battaryi }; }
        }
    }

    public class Tests
    {
        public string name;
        public string SQLname;
        bool enable = false;

        public Tests(string test_name, string SQL_name)
        {
            SQLname = SQL_name;
            name = test_name;
        }

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }
    }

    public class All_Tests
    {
        public Tests tu = new Tests("Проверка ТУ", "tu");
        public Tests kf = new Tests("Проверка KF", "kf");
        public Tests tc = new Tests("Проверка TC", "tc");
        public Tests din = new Tests("Проверка Din", "din");
        public Tests entu = new Tests("Проверка EnTU", "entu");
        public Tests tc12v = new Tests("Проверка 12B TC", "tc12v");
        public Tests power = new Tests("Проверка питания", "current");
        public Tests tuMTU5 = new Tests("Проверка ТУ MTU5", "MTU5_TU");
        public Tests current0 = new Tests("Проверка ток 0", "current0");
        public Tests battary = new Tests("Проверка заряда батареи", "battary");
        public Tests powerMTU5 = new Tests("Проверка питания MTU5", "MTU5_Power");
        public Tests temperature = new Tests("Проверка температуры", "temperature");

        public List<Tests> Test
        {
            get { return new List<Tests>() { tu, kf, tc, din, entu, tc12v, power, tuMTU5, current0, powerMTU5, temperature, battary }; }
        }
    }

    public class Module_Setup
    {
        public string name = "";
        public int power_chanel = 0;
        public int exchange_chanel = 0;

        public Min_Max_None din;
        public Min_Max_None kf;
        public Min_Max_None tc;
        public Min_Max_None tc12v;
        public Min_Max_None current;
        public Min_Max_None power;

        public All_Tests all_tests;
        public All_Registers all_registers;

        public Module_Setup(string module_name)
        {
            power_chanel = 0;
            exchange_chanel = 0;
            name = module_name;
            all_tests = new All_Tests();
            all_registers = new All_Registers();
            kf = new Min_Max_None();
            tc = new Min_Max_None();
            din = new Min_Max_None();
            power = new Min_Max_None();
            tc12v = new Min_Max_None();
            current = new Min_Max_None();
        }

        //public Module_Setup(string module_name, int power_chanel_count, int exchange_chanel_count, Min_Max_None module_current, Min_Max_None DIN, Min_Max_None KF, Min_Max_None TC, Min_Max_None TC12V)
        //{
        //    all_registers = new All_Registers();
        //    all_tests = new All_Tests();
        //    power = new Min_Max_None();
        //    current = module_current;
        //    tc12v = TC12V;
        //    din = DIN;
        //    kf = KF;
        //    tc = TC;
        //    name = module_name;
        //    power_chanel = power_chanel_count;
        //    exchange_chanel = exchange_chanel_count;
        //}
    }

    public class Send_Data
    {
        public Registers data;
        public ModBus_Libra port;

        public Send_Data(Registers using_data, ModBus_Libra using_port)
        {
            data = using_data;
            port = using_port;
        }

        public void sending()
        {
            port.Transmit(data.Register);
        }
    }
}
