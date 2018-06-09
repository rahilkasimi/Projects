using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Calculator.ThisAppClasses.MarketClasses;
using Calculator.ThisAppClasses.CurrencyClasses;
using System.Configuration;
using System.Security.AccessControl;
using System.Security.Principal;


namespace CalculatorApplication
{
    public partial class Form1 : Form
    {
        //Code below is for form movement (moveform method will be called on form objects to be moved) --------
        private const int HT_CAPTION = 0x2;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern bool ReleaseCapture();
        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        //----------------------------------------------

        // Constructors --------------------------------------------
        Calculate calculate = new Calculate();
        CalculateCurrency calculateCurrency = new CalculateCurrency();
        StockQuoteFileOperations sqFileOperations = new StockQuoteFileOperations();
        // End of Constructors --------------------------------------------

        // Variables --------------------------------------------
        bool dotClicked, operatorClicked, equalPressed = false;
        List<string> lstNumbers = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "." };
        List<string> lstOperators = new List<string> { "+", "-", "*", "/", "X", "%" };
        List<string> lstEqualOperators = new List<string> { "=" };
        List<string> listFileReadGlobal = new List<string>();
        // List<object> lstOtherOperators = new List<object> { Keys.Enter, Keys.Back, Keys.Up, Keys.Down, Keys.Right, Keys.Left };
        List<string> lstHistoryCalculationsGlobal = new List<string>();
        List<string> lstRatesGlobal = new List<string>();
        // string[] lastEquation;
        int currentFormLocation, formSizeWidth = 0;
        int roundDigit = 10;
        //End of Variables --------------------------------------------

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up: // left arrow key
                    if (lstHistoryCalculationsGlobal.Count != 0)
                    {
                        lblEquationDisplay.Text = lstHistoryCalculationsGlobal[lstHistoryCalculationsGlobal.Count - 1];
                    }
                    return true;
                case Keys.Down: // right arrow key
                    //MessageBox.Show("Right");
                    return true;
                case Keys.Enter: // Enter key
                    EnterKeyFunctionProcess();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void EnterKeyFunctionProcess()
        {
            CalculateResult();
            //    btnConvertCurrency.PerformClick();
            //  btnStockUpdateLatest.PerformClick();
        }
        public Form1()
        {
            InitializeComponent();
            lblEquationDisplay.Focus();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ListViewPopulate();
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.tabControl1.SelectedTab == this.tabControl1.TabPages["tbpCalculator"])
            {
                if (lstNumbers.Contains(e.KeyChar.ToString()))
                {
                    KeyEntered(e.KeyChar.ToString());
                }
                if (lblEquationDisplay.Text == string.Empty && e.KeyChar.ToString() == "-")
                {
                    lblEquationDisplay.Text = "-";
                    //lblEquationDisplay.Text = lblEquationDisplay.Text.Substring(0, lblEquationDisplay.Text.Length - 1);
                }
                if (lstOperators.Contains(e.KeyChar.ToString()) && lblEquationDisplay.Text != string.Empty)
                {
                    string operator1 = e.KeyChar.ToString();
                    if (operator1 == "*") { operator1 = "X"; }
                    OperatorSelected(operator1);
                }
                if (lstEqualOperators.Contains(e.KeyChar.ToString()))
                {
                    CalculateResult();
                    btnConvertCurrency.PerformClick();
                }
                //if (e.KeyChar == (int)Keys.Enter) { CalculateResult();  }
                if (e.KeyChar == (int)Keys.Escape)
                {
                    if (lblEquationDisplay.Text == string.Empty)
                    {
                        if (MessageBox.Show("Do you want to exit? ", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            Application.Exit();
                        }
                    }
                    else { btnClear.PerformClick(); }
                }
                if (e.KeyChar == (int)Keys.Back) { btnBack.PerformClick(); }
            }
        }
        private void MoveForm(MouseEventArgs e)  //This code is for form movement 
        {
            try
            {
                base.OnMouseDown(e);
                if (e.Button == MouseButtons.Left)
                {
                    System.Drawing.Rectangle rct = DisplayRectangle;
                    if (rct.Contains(e.Location))
                    {
                        ReleaseCapture();
                        SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    }
                }
            }
            catch { }
        }  //End of code for form movement 

        //Calculator Methods ---------------------------------
        private void btnBack_Click(object sender, EventArgs e)
        {
            int i = 0;

            if (lblEquationDisplay.Text != string.Empty)
                if (lblEquationDisplay.Text.Substring(lblEquationDisplay.Text.Length - 1, 1) == " ")
                {
                    i = 3;
                    lblEquationDisplay.Text = lblEquationDisplay.Text.Substring(0, lblEquationDisplay.Text.Length - i);
                }
                else
                {
                    i = 1;
                    lblEquationDisplay.Text = lblEquationDisplay.Text.Substring(0, lblEquationDisplay.Text.Length - i);
                }
        }
        private void ButtonClick(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (lstNumbers.Contains(b.Text)) { KeyEntered(b.Text); }
            lblEquationDisplay.Focus();
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            lblEquationDisplay.Text = string.Empty;
            lblResult.Text = string.Empty;
            lblSimultaneousResult.Text = string.Empty;
            dotClicked = false;
        }
        private void btnEqual_Click(object sender, EventArgs e)
        {
            CalculateResult();
        }
        private void btnOff_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btnRightPanel_Click(object sender, EventArgs e)
        {
            currentFormLocation = formSizeWidth;
            if (Form.ActiveForm.Width > formSizeWidth)
            {
                Form.ActiveForm.Width = formSizeWidth;
                //this.CenterToScreen(); //this code is working for screen center
                //btnRightPanel.BackgroundImage = btnRightPanel.BackgroundImage = Image.FromFile("");
                grbCurrencyConverter.Visible = false;
            }
            else
            {
                timer2.Start();
                Form.ActiveForm.Location = new Point(250, ActiveForm.Location.Y);
                //getCurrencyUsingJQuery();
                grbCurrencyConverter.Visible = true;
                getCurrencyUsingJQuery();
                txtAmountToConvert.Text = string.Empty;
            }
            //Form1.ActiveForm.Width = 800;
            //if(Form1.ActiveForm.SizeChanged == true)
            //    FormStartPosition.CenterScreen;
        }
        private void lblEquationDisplay_TextChanged(object sender, EventArgs e)
        {
            if (lblEquationDisplay.Text.Length > 15)
            {
                lblEquationDisplay.Font = new System.Drawing.Font("Arial", 14);
            }
            else
            {
                lblEquationDisplay.Font = new System.Drawing.Font("Tahoma", 20);
            }
            if (lblEquationDisplay.Text.Contains(" "))
            //(lblEquationDisplay.Text.Contains("+") || lblEquationDisplay.Text.Contains("-") || lblEquationDisplay.Text.Contains("X") || lblEquationDisplay.Text.Contains("/"))
            {
                operatorClicked = true;
            }
            else operatorClicked = false;

            CalculateSimultaneousResult();
        }
        private void panel1_MouseDown_2(object sender, MouseEventArgs e)
        {
            MoveForm(e);
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            btnStockUpdateLatest.PerformClick();
            //if (currentFormLocation > 690)
            //{
            //    timer2.Stop();
            //}
            //Form.ActiveForm.Width = currentFormLocation + 10;
            //panel1.Width = panel2.Width = currentFormLocation + 10;
            //currentFormLocation += 10;
        }
        private void KeyEntered(string keyEntered)
        {
            int i = 0;
            if (operatorClicked == true) { dotClicked = false; i = 2; }
            if (keyEntered == "." && !lblEquationDisplay.Text.Split(' ')[i].Contains(".") && dotClicked == false)
            {
                lblEquationDisplay.Text = lblEquationDisplay.Text + keyEntered;
                dotClicked = true;
            }
            if (keyEntered != ".")
            {
                lblEquationDisplay.Text = lblEquationDisplay.Text + keyEntered;
            }
        }
        private void OperatorSelected(string operatorText)
        {
            // foreach (string item in lstOperators)
            //{

            if (lblEquationDisplay.Text.Contains(" "))
            {

                string[] row = lblEquationDisplay.Text.Split(' ');
                if (row[2] != string.Empty)
                {
                    CalculateResult();

                    //operatorClicked = false;
                }
                else if (lblEquationDisplay.Text.Contains(" ") && operatorText == "-")
                {
                    lblEquationDisplay.Text = lblEquationDisplay.Text + "-";

                }
            }
            //}
            if (operatorClicked == false)
            {
                if (lblEquationDisplay.Text.Contains(" ") && operatorText == "-")
                {
                    //lblEquationDisplay.Text += " " + operatorText + " " + "-";
                }
                else
                {
                    lblEquationDisplay.Text += " " + operatorText + " ";
                    // operatorClicked = true;
                }
            }

        }
        private void operatorSelectedViaClick(object sender, EventArgs e)
        {
            string b = (sender as Button).Text;
            if (lblEquationDisplay.Text != string.Empty && lstOperators.Contains(b))
            {
                OperatorSelected(b);
                lblEquationDisplay.Focus();
            }
        }
        private void CalculateResult()
        {
            try
            {
                if (equalPressed == true) { }
                if (operatorClicked == true && lblEquationDisplay.Text.Split(' ')[2] != string.Empty)
                {
                    if (lblEquationDisplay.Text.Split(' ')[1] == "/" && lblEquationDisplay.Text.Split(' ')[2] == "0")
                    {
                        lstHistoryCalculationsGlobal.Add(lblEquationDisplay.Text);
                        lblResult.Text = "Divide by {0} error";
                    }
                    else
                    {
                        lstHistoryCalculationsGlobal.Add(lblEquationDisplay.Text);
                        string[] row = lblEquationDisplay.Text.Split(' ');

                        double result = calculate.CalculateAnswer(row[1], roundDigit, Convert.ToDouble(row[0]), Convert.ToDouble(row[2]));
                        lblEquationDisplay.Text = result.ToString();
                        lblResult.Text = result.ToString();
                        dotClicked = false; 
                        equalPressed = true;
                        lblResult.Text = string.Empty;
                    }
                }
                else { }
            }
            catch (Exception ex) { MessageBox.Show(ex.StackTrace); }
        }
        private void CalculateSimultaneousResult()
        {
            if (operatorClicked == true)
            {
                string[] row = lblEquationDisplay.Text.Split(' ');
                try
                {
                    if (row[0] != string.Empty && row[1] != string.Empty && row[2] != string.Empty)
                    {
                        double result = calculate.CalculateAnswer(row[1], roundDigit, Convert.ToDouble(row[0]), Convert.ToDouble(row[2]));
                        lblSimultaneousResult.Text = result.ToString();
                    }
                }
                catch { }
            }
        }
        // End of Calculator Methods ---------------------------------

        // Currency Related Methods ------------------------------------
        private async void getCurrencyUsingJQuery()
        {
            string URL = "http://data.fixer.io/api/latest?access_key=b99f2af57916e691e2d9a9f6775c39f0";
            /*string URL2 = "http://www.apilayer.net/api/live?access_key=61404188860f4964177c30b2c3cfb9f6&format=1";*/
            string DATA = @"{""object"":{""val"":""Name""}}";
            //----------------------------------------------------------------------------
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = DATA.Length;
            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            requestWriter.Write(DATA);
            requestWriter.Close();
            WebResponse webResponse = request.GetResponse();
            Stream webStream = webResponse.GetResponseStream();
            StreamReader responseReader = new StreamReader(webStream);
            string response = responseReader.ReadToEnd();
            //string response2 = response.ToUpperInvariant();
            //textBox1.Text = response;
            string[] fullReply = response.Split(',');
            string[] rates = response.Split('{');
            string[] ratesDivided = rates[2].Replace("\"", "").Replace("}", "").Trim().Split(',');
            List<string> tempListCurrencyRatesGlobal = new List<string>();

            foreach (string item in ratesDivided)
            {
                lstRatesGlobal.Add(item);
                //cmbFromCurrency.Items.Add(item);
                //cmbToCurrency.Items.Add(item);
                if (cmbFromCurrency.Items.Contains(item.Split(':')[0]) == true)
                {
                    cmbFromCurrency.Items.Add(item.Split(':')[0]);
                    cmbToCurrency.Items.Add(item.Split(':')[0]);
                }
            }
            lblLastCurrencyUpdateDateResponse.Text = fullReply[3].Split(':')[1].Replace("\"", "");
            // cmbFromCurrency.Text = fullReply[2].Split(':')[1].Replace("\"", "");
            responseReader.Close();
            //----------------------------------------------------------------------------
        }
        private void tbpCurrencyRates_Enter_1(object sender, EventArgs e)
        {
            if (lstRatesGlobal.Count < 1)
            {
                getCurrencyUsingJQuery();
            }
        }
        private void txtAmountToConvert_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
        private void txtAmountToConvert_TextChanged(object sender, EventArgs e)
        {
            btnConvertCurrency.PerformClick();
        }
        private void cmbFromCurrency_TextChanged_1(object sender, EventArgs e)
        {
            if (txtAmountToConvert.Text != string.Empty)
            {
                btnConvertCurrency.PerformClick();
            }
            else lblCurrencyConvertResult.Text = string.Empty;
        }
        private void btnConvertCurrency_Click(object sender, EventArgs e)
        {
            if (txtAmountToConvert.Text != string.Empty)
            {
                if (cmbFromCurrency.Text != string.Empty && cmbToCurrency.Text != string.Empty)
                {
                    double resultCurrency, currencyExchangeRate;
                    ConvertCurrency(out resultCurrency, out currencyExchangeRate);
                    lblCurrencyConvertResult.Text = resultCurrency.ToString();
                    lblExchangeRate.Text = $"1 {cmbFromCurrency.Text.Substring(0, 3)} = {currencyExchangeRate} {cmbToCurrency.Text.Substring(0, 3)}";
                }
            }
        }
        private void ConvertCurrency(out double resultCurrency, out double currencyExchangeRate)
        {
            double fromCurrency = Convert.ToDouble(lstRatesGlobal.Find(x => x.Contains(cmbFromCurrency.Text.Substring(0, 3))).Split(':')[1]);
            double toCurrency = Convert.ToDouble(lstRatesGlobal.Find(x => x.Contains(cmbToCurrency.Text.Substring(0, 3))).Split(':')[1]);
            resultCurrency = calculateCurrency.ConvertAmount(Convert.ToDouble(txtAmountToConvert.Text), fromCurrency, toCurrency);
            currencyExchangeRate = calculateCurrency.ExchangeRate(fromCurrency, toCurrency);
        }
        private void btnCurrencySwap_Click_1(object sender, EventArgs e)
        {
            string temp = cmbFromCurrency.Text;
            cmbFromCurrency.Text = cmbToCurrency.Text;
            cmbToCurrency.Text = temp;
        }
        private void btnCurrencyUpdate_Click(object sender, EventArgs e)
        {
            getCurrencyUsingJQuery();
        }
        private void btnCurrencyUpdateTest_Click(object sender, EventArgs e)
        {
            //CurrencySymbolsFileOperations currencySymbolsFile = new CurrencySymbolsFileOperations();
            //currencySymbolsFile.Read()
        }

        // End of Currency Related Methods ------------------------------------

        // Stock Market Related Methods ------------------------------------
        private async void btnStockUpdateLatest_ClickAsync(object sender, EventArgs e)
        {
            await StockUpdateLatest(cmbExchangeOrStock.Text);
        }
        private void ListViewPopulate()
        {
            List<string> listFileRead = sqFileOperations.ReadFile();
            lsvStockNamesAndSymbols.Items.Clear();
            cmbExchangeOrStock.Items.Clear();
            foreach (string item in listFileRead)
            {
                string[] row = item.Split(('~'));
                ListViewItem listViewItem = new ListViewItem(row);
                lsvStockNamesAndSymbols.Items.Add(listViewItem);
                cmbExchangeOrStock.Items.Add(item);
            }
        }
        private void chkContinuousMarketUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt16(cmbStockUpdateDurationInSeconds.Text) < 1 || cmbStockUpdateDurationInSeconds.Text == string.Empty) { cmbStockUpdateDurationInSeconds.Text = "5"; }
            if (chkContinuousMarketUpdate.Checked == true)
            {
                cmbExchangeOrStock.Enabled = false;
                cmbStockUpdateDurationInSeconds.Enabled = false;
                timer2.Interval = Convert.ToInt16(cmbStockUpdateDurationInSeconds.Text) * 1000;
                timer2.Start();
            }
            else
            {
                cmbExchangeOrStock.Enabled = true;
                cmbStockUpdateDurationInSeconds.Enabled = true;
                timer2.Stop();
            }
        }
        private async void cmbExchangeOrStock_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            await StockUpdateLatest(cmbExchangeOrStock.Text);
        }
        private async System.Threading.Tasks.Task StockUpdateLatest(string ticker)
        {
            try
            {
                if (ticker.Contains("~")) { ticker = ticker.Split('~')[0]; } else { ticker = ticker.Trim(); }
                YahooFinanceApi.Security sensex = await MarketUpdate.StockUpdate(ticker);
                lblRegularMarketPriceUpdate.Text = $"{Math.Round(sensex.RegularMarketPrice, 2)}";
                lblRatesUpdateDifference.Text = $"{Math.Round(sensex.RegularMarketChange, 2)} ({Math.Round(sensex.RegularMarketChangePercent, 2)}%)";
                if (sensex.RegularMarketChange > 0)
                {
                    lblRatesUpdateDifference.ForeColor = Color.LightGreen;
                    lblRatesUpdateDifference.Text = $"+{lblRatesUpdateDifference.Text}";
                }
                else if (sensex.RegularMarketChange < 0) { lblRatesUpdateDifference.ForeColor = Color.Pink; }
                else { lblRatesUpdateDifference.ForeColor = Color.White; }
                long dateNumber = sensex.RegularMarketTime;
                long beginTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
                DateTime dt = new DateTime(beginTicks + dateNumber * 10000000, DateTimeKind.Utc);
                lblRatesUpdateDate.Text = dt.ToShortDateString();
                lblMarketName.Text = sensex.ShortName.ToString();
                lsbMarketDetails.Items.Clear();
                lsbMarketDetails.Items.Add($"RegularMarketChange:  {sensex.RegularMarketChange.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketChangePercent:  {sensex.RegularMarketChangePercent.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketDayHigh:  {sensex.RegularMarketDayHigh.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketDayLow:  {sensex.RegularMarketDayLow.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketOpen:  {sensex.RegularMarketOpen.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketPreviousClose:  {sensex.RegularMarketPreviousClose.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketPrice:  {sensex.RegularMarketPrice.ToString()}");
                lsbMarketDetails.Items.Add($"RegularMarketTime:{dt.ToLocalTime().ToString()}");
                //lsbMarketDetails.Items.Add($"RegularMarketVolume:  {sensex.RegularMarketVolume.ToString()}");
                lsbMarketDetails.Items.Add($"ShortName:  {sensex.ShortName.ToString()}");

                lsbMarketDetails.Items.Add($"ExchangeTimezoneName:  {sensex.ExchangeTimezoneName.ToString()}");
                lsbMarketDetails.Items.Add($"ExchangeTimezoneShortName:  {sensex.ExchangeTimezoneShortName.ToString()}");
                lsbMarketDetails.Items.Add($"SourceInterval:  {sensex.SourceInterval.ToString()}");
                lsbMarketDetails.Items.Add($"Symbol:  {sensex.Symbol.ToString()}");
                //lsbMarketDetails.Items.Add($"SharesOutstanding:  {sensex.SharesOutstanding.ToString()}");
                //lsbMarketDetails.Items.Add($"Ask:  {sensex.Ask.ToString()}");
                //lsbMarketDetails.Items.Add($"AskSize:  {sensex.AskSize.ToString()}");
                //lsbMarketDetails.Items.Add($"AverageDailyVolume10Day:  {sensex.AverageDailyVolume10Day.ToString()}");
                //lsbMarketDetails.Items.Add($"AverageDailyVolume3Month:  {sensex.AverageDailyVolume3Month.ToString()}");
                //lsbMarketDetails.Items.Add($"Bid:  {sensex.Bid.ToString()}");
                //lsbMarketDetails.Items.Add($"BidSize:  {sensex.BidSize.ToString()}");
                //lsbMarketDetails.Items.Add($"BookValue:  {sensex.BookValue.ToString()}");
                //lsbMarketDetails.Items.Add($"Currency:  {sensex.Currency.ToString()}");
                //lsbMarketDetails.Items.Add($"DividendDate:  {sensex.DividendDate.ToString()}");
                //lsbMarketDetails.Items.Add($"EarningsTimestamp:  {sensex.EarningsTimestamp.ToString()}");
                //lsbMarketDetails.Items.Add($"EarningsTimestampEnd:  {sensex.EarningsTimestampEnd.ToString()}");
                //lsbMarketDetails.Items.Add($"EarningsTimestampStart:  {sensex.EarningsTimestampStart.ToString()}");
                //lsbMarketDetails.Items.Add($"EpsForward:  {sensex.EpsForward.ToString()}");
                //lsbMarketDetails.Items.Add($"EpsTrailingTwelveMonths:  {sensex.EpsTrailingTwelveMonths.ToString()}");
                //lsbMarketDetails.Items.Add($"Exchange:  {sensex.Exchange.ToString()}");
                // lsbMarketDetails.Items.Add($"ExchangeDataDelayedBy:  {sensex.ExchangeDataDelayedBy.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyDayAverage:  {sensex.FiftyDayAverage.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyDayAverageChange:  {sensex.FiftyDayAverageChange.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyDayAverageChangePercent:  {sensex.FiftyDayAverageChangePercent.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyTwoWeekHigh:  {sensex.FiftyTwoWeekHigh.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyTwoWeekHighChange:  {sensex.FiftyTwoWeekHighChange.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyTwoWeekHighChangePercent:  {sensex.FiftyTwoWeekHighChangePercent.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyTwoWeekLow:  {sensex.FiftyTwoWeekLow.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyTwoWeekLowChange:  {sensex.FiftyTwoWeekLowChange.ToString()}");
                //lsbMarketDetails.Items.Add($"FiftyTwoWeekLowChangePercent:  {sensex.FiftyTwoWeekLowChangePercent.ToString()}");
                //lsbMarketDetails.Items.Add($"FinancialCurrency:  {sensex.FinancialCurrency.ToString()}");
                //lsbMarketDetails.Items.Add($"ForwardPE:  {sensex.ForwardPE.ToString()}");
                //lsbMarketDetails.Items.Add($"FullExchangeName:  {sensex.FullExchangeName.ToString()}");
                //lsbMarketDetails.Items.Add($"GmtOffSetMilliseconds:  {sensex.GmtOffSetMilliseconds.ToString()}");
                //lsbMarketDetails.Items.Add($"Language:  {sensex.Language.ToString()}");
                //lsbMarketDetails.Items.Add($"LongName:  {sensex.LongName.ToString()}");
                //lsbMarketDetails.Items.Add($"Market:  {sensex.Market.ToString()}");
                //lsbMarketDetails.Items.Add($"MarketCap:  {sensex.MarketCap.ToString()}");
                //lsbMarketDetails.Items.Add($"MarketState:  {sensex.MarketState.ToString()}");
                //lsbMarketDetails.Items.Add($"MessageBoardId:  {sensex.MessageBoardId.ToString()}");
                //lsbMarketDetails.Items.Add($"PriceHint:  {sensex.PriceHint.ToString()}");
                //lsbMarketDetails.Items.Add($"PriceToBook:  {sensex.PriceToBook.ToString()}");
                //lsbMarketDetails.Items.Add($"QuoteSourceName:  {sensex.QuoteSourceName.ToString()}");
                //lsbMarketDetails.Items.Add($"QuoteType:  {sensex.QuoteType.ToString()}");
                //lsbMarketDetails.Items.Add($"Tradeable:  {sensex.Tradeable.ToString()}");
                //lsbMarketDetails.Items.Add($"TrailingAnnualDividendRate:  {sensex.TrailingAnnualDividendRate.ToString()}");
                //lsbMarketDetails.Items.Add($"TrailingAnnualDividendYield:  {sensex.TrailingAnnualDividendYield.ToString()}");
                //lsbMarketDetails.Items.Add($"TrailingPE:  {sensex.TrailingPE.ToString()}");
                //lsbMarketDetails.Items.Add($"TwoHundredDayAverage:  {sensex.TwoHundredDayAverage.ToString()}");
                //lsbMarketDetails.Items.Add($"TwoHundredDayAverageChange:  {sensex.TwoHundredDayAverageChange.ToString()}");
                //lsbMarketDetails.Items.Add($"TwoHundredDayAverageChangePercent:  {sensex.TwoHundredDayAverageChangePercent.ToString()}");
            }
            catch (Exception ex) { MessageBox.Show(ex.StackTrace); }
        }
        private void btnDeleteStock_Click_1(object sender, EventArgs e)
        {
            sqFileOperations.DeleteFromFile(lsvStockNamesAndSymbols.SelectedItems[0].SubItems[0].Text, lsvStockNamesAndSymbols.SelectedItems[0].SubItems[1].Text);
            //  ConfigurationManager.AppSettings.Remove(lsvStockNamesAndSymbols.SelectedItems[0].SubItems[0].Text);
            ListViewPopulate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string fromCurrency = string.Empty;
            //string toCurrency = "GBP";
            //string[] currencySymbols = File.ReadAllLines("CurrencySymbols.txt");
            //List<string> currencies = new List<string>();


            //AvapiCurrency avapiCurrency = new AvapiCurrency();
            //int count = 0;

            //foreach (string item in currencySymbols)
            //{
            //    if (count > 10) break;
            //    try
            //    {
            //        richTextBox1.Text += avapiCurrency.AvapiMain(null, null);
            //            //item.Substring(0, 3), toCurrency);
            //        count++;
            //    }
            //    catch { }
            //}
        }

        private async void btnAddStock_ClickAsync(object sender, EventArgs e)
        {
            string ticker = txtMarketQuote.Text;
            try
            {
                YahooFinanceApi.Security sensex = await MarketUpdate.StockUpdate(ticker);
                sqFileOperations.AddInFile(sensex.Symbol.ToString(), sensex.ShortName.ToString());
                //  ConfigurationManager.AppSettings.Add(sensex.Symbol.ToString(), sensex.ShortName.ToString());
            }
            catch (Exception ex) { MessageBox.Show("Quote not found"); }
            ListViewPopulate();
        }
        private void lsvStockNamesAndSymbols_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvStockNamesAndSymbols.SelectedItems == null) return;
        }
        private void lsvStockNamesAndSymbols_DoubleClick_1(object sender, EventArgs e)
        {
            cmbExchangeOrStock.Text = $"{lsvStockNamesAndSymbols.SelectedItems[0].SubItems[0].Text}~{lsvStockNamesAndSymbols.SelectedItems[0].SubItems[1].Text}";
            this.tabControl1.SelectedTab = this.tabControl1.TabPages["tbpMarkets"];
            btnStockUpdateLatest.PerformClick();
        }
        // End of Stock Market Related Methods ------------------------------------       
    }
}
