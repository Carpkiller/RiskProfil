using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace RiskProfil
{
    public partial class Form1 : Form
    {
        private List<ContractRiadok> zoznamHodnot;

        public Form1()
        {
            InitializeComponent();

            listView1.Columns.Add("Call sell");
            listView1.Columns.Add("Call buy");
            listView1.Columns.Add("Strike");
            listView1.Columns.Add("Put sell");
            listView1.Columns.Add("Put buy");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            zoznamHodnot = new List<ContractRiadok>();
            double aktCena = double.Parse(textBox1.Text);
            for (double i = aktCena-2; i < aktCena + 2; i=i+0.5)
            {
                zoznamHodnot.Add(new ContractRiadok()
                {
                    Strike = i,
                    Podklad = "CL",
                    CallBuyCena = 0,
                    CallSellCena = 0,
                    PutBuyCena = 0,
                    PutSellCena = 0
                });
            }

            foreach (var contractRiadok in zoznamHodnot)
            {
                string[] row = { "", "", contractRiadok.Strike.ToString(CultureInfo.InvariantCulture), "", "" };
                var listViewItem = new ListViewItem(row);
                listView1.Items.Add(listViewItem);
            }
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            string hodnota = "0";

            Point mousePos = listView1.PointToClient(MousePosition);
            var info = listView1.HitTest(mousePos);
            var row = info.Item.Index;
            var col = info.Item.SubItems.IndexOf(info.SubItem);

            using (var form = new InputDialogBox())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    hodnota = form.ReturnValue1; //values preserved after close
                }
            }
            double priemCena;
            if (col == 0)
            {
                var val = info.Item.SubItems[col].Text == "" ? 0 : zoznamHodnot[row].PocetCallSell; //int.Parse(info.Item.SubItems[col].Text);
                priemCena = PrepocitajCenu(double.Parse(hodnota), zoznamHodnot[row].CallSellCena, zoznamHodnot[row].PocetCallSell);
                info.Item.SubItems[col].Text = $"{priemCena.ToString("F")}({(val - 1)})";
                zoznamHodnot[row].CallSellCena = priemCena;
                zoznamHodnot[row].PocetCallSell = val - 1;
                zoznamHodnot[row].MaObchod = true;
            }
            
            if (col == 3)
            {
                var val = info.Item.SubItems[col].Text == "" ? 0 : zoznamHodnot[row].PocetPutSell; // int.Parse(info.Item.SubItems[col].Text);
                priemCena = PrepocitajCenu(double.Parse(hodnota), zoznamHodnot[row].PutSellCena, zoznamHodnot[row].PocetPutSell);
                info.Item.SubItems[col].Text = $"{priemCena.ToString("F")}({(val - 1)})";
                zoznamHodnot[row].PutSellCena = priemCena;
                zoznamHodnot[row].PocetPutSell = val - 1;
                zoznamHodnot[row].MaObchod = true;
            }

            if (col == 1)
            {
                var val = info.Item.SubItems[col].Text == "" ? 0 : zoznamHodnot[row].PocetCallBuy; // int.Parse(info.Item.SubItems[col].Text);
                priemCena = PrepocitajCenu(double.Parse(hodnota), zoznamHodnot[row].CallBuyCena, zoznamHodnot[row].PocetCallBuy);
                info.Item.SubItems[col].Text = $"{priemCena.ToString("F")}({(val + 1)})";
                zoznamHodnot[row].CallBuyCena = priemCena;
                zoznamHodnot[row].PocetCallBuy = val + 1;
                zoznamHodnot[row].MaObchod = true;
            }
            if (col == 4)
            {
                var val = info.Item.SubItems[col].Text == "" ? 0 : zoznamHodnot[row].PocetPutBuy; // int.Parse(info.Item.SubItems[col].Text);
                priemCena = PrepocitajCenu(double.Parse(hodnota), zoznamHodnot[row].PutBuyCena, zoznamHodnot[row].PocetPutBuy);
                info.Item.SubItems[col].Text = $"{priemCena.ToString("F")}({(val + 1)})";
                zoznamHodnot[row].PutBuyCena = priemCena;
                zoznamHodnot[row].PocetPutBuy = val + 1;
                zoznamHodnot[row].MaObchod = true;
            }

            listView1.Refresh();
        }

        private double PrepocitajCenu(double inputCena, double predchCena, int pocetKontraktov)
        {
            return ((predchCena * Math.Abs(pocetKontraktov)) + inputCena)/(Math.Abs(pocetKontraktov) + 1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = string.Empty;
            
            var strategie = IdentifikujStrategiu();

            foreach (var strategia in strategie)
            {
                textBox2.AppendText(strategia.Item1 + "  "+ strategia.Item2);
                textBox2.AppendText(Environment.NewLine);
            }
        }

        private List<Tuple<Strategia, string>> IdentifikujStrategiu()
        {
            var result = new List<Tuple<Strategia, string>>();

            var pocetPutKontraktov = GetPocetPutKontraktov();
            var pomZoznHodnot = zoznamHodnot.ToClonedList();
            pomZoznHodnot.AddRange(zoznamHodnot);

            for (int i = 0; i < pomZoznHodnot.Count; i++)
            {
                int pocetKontraktov;
                double cena;
                double be1;

                if (Math.Abs(pomZoznHodnot[i].PocetPutBuy) > 0)
                {
                    for (int j = i; j < pomZoznHodnot.Count; j++)
                    {
                        if (Math.Abs(pomZoznHodnot[j].PocetPutSell) > 0)
                        {
                            if (Math.Abs(pomZoznHodnot[i].PocetPutBuy) == Math.Abs(pomZoznHodnot[j].PocetPutSell))
                            {
                                pocetKontraktov = Math.Abs(pomZoznHodnot[i].PocetPutBuy);
                                cena = pomZoznHodnot[j].PutSellCena - pomZoznHodnot[i].PutBuyCena;
                                be1 = pomZoznHodnot[j].Strike - cena;
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[i].PocetPutBuy);
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[j].PocetPutSell);
                                pomZoznHodnot[i].PocetPutBuy = 0;
                                pomZoznHodnot[j].PocetPutSell = 0;
                                result.Add(new Tuple<Strategia, string>(Strategia.CreditPutSpread,
                                    $"{pomZoznHodnot[i].Strike} - {pomZoznHodnot[j].Strike} , pocet kontraktov - {pocetKontraktov}, " +
                                    $"cena  - {Math.Abs(cena)}C , BE bod - {be1}+"));
                                break;
                            }
                            if (Math.Abs(pomZoznHodnot[i].PocetPutBuy) < Math.Abs(pomZoznHodnot[j].PocetPutSell))
                            {
                                pocetKontraktov = Math.Abs(pomZoznHodnot[j].PocetPutBuy);
                                cena = pomZoznHodnot[i].PutSellCena - pomZoznHodnot[j].PutBuyCena;
                                be1 = pomZoznHodnot[j].Strike - cena;
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[i].PocetPutBuy);
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[j].PocetPutSell);
                                pomZoznHodnot[j].PocetPutSell = pomZoznHodnot[j].PocetPutSell - pomZoznHodnot[i].PocetPutBuy;
                                pomZoznHodnot[i].PocetPutBuy = 0;
                                result.Add(new Tuple<Strategia, string>(Strategia.CreditPutSpread,
                                    $"{pomZoznHodnot[i].Strike} - {pomZoznHodnot[j].Strike} , pocet kontraktov - {pocetKontraktov}, " +
                                    $"cena  - {Math.Abs(cena)}C , BE bod - {be1}+"));
                                break;
                            }
                            if (Math.Abs(pomZoznHodnot[i].PocetPutBuy) > Math.Abs(pomZoznHodnot[j].PocetPutSell))
                            {
                                pocetKontraktov = Math.Abs(pomZoznHodnot[j].PocetPutBuy);
                                cena = pomZoznHodnot[i].PutSellCena - pomZoznHodnot[j].PutBuyCena;
                                be1 = pomZoznHodnot[j].Strike - cena;
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[i].PocetPutBuy);
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[j].PocetPutSell);
                                pomZoznHodnot[i].PocetPutBuy = pomZoznHodnot[i].PocetPutBuy - pomZoznHodnot[j].PocetPutSell;
                                pomZoznHodnot[j].PocetPutSell = 0;
                                result.Add(new Tuple<Strategia, string>(Strategia.CreditPutSpread,
                                    $"{pomZoznHodnot[i].Strike} - {pomZoznHodnot[j].Strike} , pocet kontraktov - {pocetKontraktov}, " +
                                    $"cena  - {Math.Abs(cena)}C , BE bod - {be1}+"));
                                continue;
                            }
                        }

                        if (pocetPutKontraktov == 0)
                        {
                            break;
                        }
                    }

                    if (pocetPutKontraktov == 0)
                    {
                        break;
                    }
                }
                if (Math.Abs(pomZoznHodnot[i].PocetPutSell) > 0)
                {
                    for (int j = i; j < pomZoznHodnot.Count; j++)
                    {
                        if (Math.Abs(pomZoznHodnot[j].PocetPutBuy) > 0)
                        {
                            if (Math.Abs(pomZoznHodnot[i].PocetPutSell) == Math.Abs(pomZoznHodnot[j].PocetPutBuy))
                            {
                                pocetKontraktov = Math.Abs(pomZoznHodnot[i].PocetPutSell);
                                cena = pomZoznHodnot[i].PutSellCena - pomZoznHodnot[j].PutBuyCena;
                                be1 = pomZoznHodnot[j].Strike - cena;
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[j].PocetPutBuy);
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[i].PocetPutSell);
                                pomZoznHodnot[i].PocetPutSell = 0;
                                pomZoznHodnot[j].PocetPutBuy = 0;
                                result.Add(new Tuple<Strategia, string>(Strategia.DebitPutSpread,
                                    $"{pomZoznHodnot[i].Strike} - {pomZoznHodnot[j].Strike} , pocet kontraktov - {pocetKontraktov}, " +
                                    $"cena  - {cena}D , BE bod - {be1}-"));
                                break;
                            }
                            if (Math.Abs(pomZoznHodnot[i].PocetPutSell) > Math.Abs(pomZoznHodnot[j].PocetPutBuy))
                            {
                                pocetKontraktov = Math.Abs(pomZoznHodnot[j].PocetPutBuy);
                                cena = pomZoznHodnot[i].PutSellCena - pomZoznHodnot[j].PutBuyCena;
                                be1 = pomZoznHodnot[j].Strike - cena;
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[j].PocetPutBuy);
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[i].PocetPutSell);
                                pomZoznHodnot[i].PocetPutSell = pomZoznHodnot[i].PocetPutSell - pomZoznHodnot[j].PocetPutBuy;
                                pomZoznHodnot[j].PocetPutBuy = 0;
                                result.Add(new Tuple<Strategia, string>(Strategia.DebitPutSpread,
                                    $"{pomZoznHodnot[i].Strike} - {pomZoznHodnot[j].Strike} , pocet kontraktov - {pocetKontraktov}, " +
                                    $"cena  - {cena}D , BE bod - {be1}-"));
                                continue;
                            }
                            if (Math.Abs(pomZoznHodnot[i].PocetPutSell) < Math.Abs(pomZoznHodnot[j].PocetPutBuy))
                            {
                                pocetKontraktov = Math.Abs(pomZoznHodnot[i].PocetPutSell);
                                cena = pomZoznHodnot[i].PutSellCena - pomZoznHodnot[j].PutBuyCena;
                                be1 = pomZoznHodnot[j].Strike - cena;
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[i].PocetPutSell);
                                pocetPutKontraktov -= Math.Abs(pomZoznHodnot[j].PocetPutBuy);
                                pomZoznHodnot[i].PocetPutSell = 0;
                                pomZoznHodnot[j].PocetPutBuy = pomZoznHodnot[j].PocetPutBuy - pomZoznHodnot[i].PocetPutSell;
                                result.Add(new Tuple<Strategia, string>(Strategia.DebitPutSpread,
                                    $"{pomZoznHodnot[i].Strike} - {pomZoznHodnot[j].Strike} , pocet kontraktov - {pocetKontraktov}, " +
                                    $"cena  - {cena}D , BE bod - {be1}-"));
                                break;
                            }
                        }

                        if (pocetPutKontraktov == 0)
                        {
                            break;
                        }
                    }

                    if (pocetPutKontraktov == 0)
                    {
                        break;
                    }
                }
            }
            pomZoznHodnot.Clear();
            return result;
        }

        private int GetPocetPutKontraktov()
        {
            return zoznamHodnot.Sum(t => Math.Abs(t.PocetPutSell) + Math.Abs(t.PocetPutBuy));
        }

        private enum Strategia
        {
            CreditPutSpread,
            DebitPutSpread,
         //   CreditCallSpread,
         //   DebitCallSpread
        }
    }
}
