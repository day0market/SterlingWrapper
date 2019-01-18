using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SterlingLib;
using System.IO;

namespace SterlingWrapper
{
    public class Connector
    {


        private STIOrder stiOrder = new STIOrder();
        private STIOrderMaint stOrdMaint = new STIOrderMaint();
        private STIOrderMaint stOrdMaint_req = new STIOrderMaint();
        private STIEvents stiEvents = new STIEvents();
        private STIPosition stiPos = new STIPosition();

        private void WriteLog(string data)
        {
            try
            {
                string path = Path.Combine("LimitLog", DateTime.Now.ToString("ddMMyyyy") + ".txt");
                if (!Directory.Exists("LimitLog")) Directory.CreateDirectory("LimitLog");

                using (StreamWriter fstream = new StreamWriter(path, true))
                {
                    fstream.WriteLine(data);

                }
            }
            catch
            {
                return;
            }
        }



        #region SendOrders

        private string DecodeError(int error)
        {
            if (error == -1) return "Invalid Account";
            if (error == -2) return "Invalid Side";
            if (error == -3) return "Invalid Qty";
            if (error == -4) return "Invalid Symbol";
            if (error == -5) return "Invalid PriceType";
            if (error == -6) return "Invalid Tif";
            if (error == -7) return "Invalid Destination";
            if (error == -8) return "Exposure Limit Violation";
            if (error == -9) return "NYSE+ Rules Violation";
            if (error == -10) return "NYSE+ 30-Second Violation";
            if (error == -11) return "Disable SelectNet Short Sales";
            if (error == -12) return "Long Sale Position Rules Violation";
            if (error == -13) return "Short Sale Position Rules Violation";
            if (error == -14) return "GTC Orders Not Enabled";
            if (error == -15) return "ActiveX API Not Enabled";
            if (error == -16) return "Sterling Trader® Pro is Offline";
            if (error == -17) return "Security Not Marked as Located";
            if (error == -18) return "Order Size Violation";
            if (error == -19) return "Position Limit Violation";
            if (error == -20) return "Buying Power /Margin Control Violation";
            if (error == -21) return "P/L Control Violation";
            if (error == -22) return "Account Not Enabled for this Product";
            if (error == -23) return "Trader Not Enabled for Futures";
            if (error == -24) return "Minimum Balance Violation";
            if (error == -25) return "Trader Not Enabled for odd lots";
            if (error == -26) return "Order dollar limit exceeded";
            if (error == -27) return "Trader Not Enabled for Options";
            if (error == -28) return "Soft share limit exceeded";
            if (error == -29) return "Loss from max profit control violation (Title builds only)";
            if (error == -30) return "Desk quantity enforcement violation";
            if (error == -31) return "Account not enabled for Sell to Open (Options)";
            if (error == -32) return "Account allowed to 'Close/Cxl' only";
            if (error == -33) return "Trader not enabled for security locating";
            if (error == -34) return "Order not able to be replaced (ReplaceOrder only)";
            if (error == -35) return "Trader not enabled for 'Buy to Cover'";
            if (error == -36) return "Invalid maturity date";
            if (error == -37) return "Only one cancel and/or replace allowed per order per second";
            if (error == -38) return "Account's maximum position value for this symbol exceeded";
            if (error == -39) return "Symbol violates the account's min/max price settings";
            if (error == -40) return "Quote Unavailable to calculate Order dollar limit";
            if (error == -41) return "Quote Unavailable to calculate Maximum Position Cost";
            if (error == -42) return "Quote Unavailable to calculate Buying Power";
            if (error == -43) return "Quote Unavailable to calculate Margin Control";
            if (error == -44) return "Floating BP Violation";
            if (error == -45) return "Market order would remove liquidity (Front end setting)";
            if (error == -46) return "Not enabled for Server Stop orders";
            if (error == -47) return "Not enabled for Trail Stop orders";
            if (error == -48) return "Order would exceed the Max Open orders per side on this symbol";
            if (error == -49) return "Quote Unavailable or Compliance threshold exceeded or quote unavailable";
            if (error == -50) return "Neither last nor Close price available for MKT order";
            if (error == -51) return "Quote Unavailable or Does not meet min average daily volume";

            return "Unknown: " + error.ToString();
        }

        public string Sendlimit(string account, string ticker, int ord_size, int ord_disp, string ord_route, double ord_price, string ord_side, string ord_tif)
        {

            stiOrder.Symbol = ticker;
            stiOrder.Account = account;
            stiOrder.Side = ord_side;
            stiOrder.Quantity = ord_size;
            stiOrder.Destination = ord_route;
            stiOrder.Tif = ord_tif;
            stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTILmt;
            stiOrder.LmtPrice = ord_price;
            stiOrder.Display = ord_disp;


            stiOrder.ClOrderID = stiOrder.Symbol + stiOrder.Side + stiOrder.Destination + stiOrder.LmtPrice + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
            int ord = stiOrder.SubmitOrder();

            string ord_id = stiOrder.ClOrderID;


            string status = "0";

            if (ord != 0)
            {
                status = DecodeError(ord);
                Console.WriteLine("Sterling API ~Limit Order: " + ord_id + " Error: " + DecodeError(ord));
            }

            string data = String.Format("{0}, {1} {2} {3} {4} status: {5} ordID: {6}", DateTime.Now.ToLongTimeString(), ticker, ord_price, ord_side, ord_route, status, ord_id);
            WriteLog(data);

            string _return = ord_id + ";" + ord;

            return _return;
        }

        public string Sendmarket(string account, string ticker, int ord_size, int ord_disp, string ord_route, string ord_side, string ord_tif)
        {
            stiOrder.Symbol = ticker;
            stiOrder.Account = account;
            stiOrder.Side = ord_side;
            stiOrder.Quantity = ord_size;
            stiOrder.Destination = ord_route;
            stiOrder.Tif = ord_tif;
            stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTIMkt;

            stiOrder.Display = ord_disp;


            stiOrder.ClOrderID = stiOrder.Symbol + stiOrder.Side + "MKT" + DateTime.Now.DayOfYear + DateTime.Now.Hour + DateTime.Now.Minute;
            int ord = stiOrder.SubmitOrder();
            string ord_id = stiOrder.ClOrderID;

            if (ord != 0)
            {
                Console.WriteLine("Sterling API ~ Market Order:" + ord_id + " Error: " + DecodeError(ord));
            }

            string _return = ord_id + ";" + ord;
            return _return;
        }

        public string Sendstop(string account, string ticker, int ord_size, int ord_disp, string ord_route, double ord_price, string ord_side, string ord_tif)
        {
            stiOrder.Symbol = ticker;
            stiOrder.Account = account;
            stiOrder.Side = ord_side;
            stiOrder.Quantity = ord_size;
            stiOrder.Destination = ord_route;
            stiOrder.Tif = ord_tif;
            stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTISvrStp;
            stiOrder.StpPrice = ord_price;
            stiOrder.Display = ord_disp;


            stiOrder.ClOrderID = stiOrder.Symbol + stiOrder.Side + "STP" + DateTime.Now.DayOfYear + DateTime.Now.Hour + DateTime.Now.Minute;
            int ord = stiOrder.SubmitOrder();
            string ord_id = stiOrder.ClOrderID;

            if (ord != 0)
            {
                Console.WriteLine("Sterling API ~ Stop Order:" + ord_id + " Error: " + DecodeError(ord));
            }

            return ord_id;
        }

        public string Sendstoplimit(string account, string ticker, int ord_size, int ord_disp, string ord_route, double stp_price, double lmt_price, string ord_side, string ord_tif)
        {
            stiOrder.Symbol = ticker;
            stiOrder.Account = account;
            stiOrder.Side = ord_side;
            stiOrder.Quantity = ord_size;
            stiOrder.Destination = ord_route;
            stiOrder.Tif = ord_tif;
            stiOrder.PriceType = SterlingLib.STIPriceTypes.ptSTISvrStpLmt;
            stiOrder.StpPrice = stp_price;
            stiOrder.LmtPrice = lmt_price;
            stiOrder.Display = ord_disp;


            stiOrder.ClOrderID = stiOrder.Symbol + stiOrder.Side + "STPLMT" + DateTime.Now.DayOfYear + DateTime.Now.Hour + DateTime.Now.Minute;
            int ord = stiOrder.SubmitOrder();
            string ord_id = stiOrder.ClOrderID;

            if (ord != 0)
            {
                Console.WriteLine("Sterling API ~ STP LMT Order:" + ord_id + " Error: " + DecodeError(ord));
            }

            string _return = ord_id + ";" + ord;
            return _return;
        }

        #endregion

        #region Cancels

        public void CancelOrder(string account, string order_id)
        {
            string new_id = order_id + "cancel";
            stOrdMaint.CancelOrder(account, 0, order_id, new_id);
            int st = OrderStatus(order_id);
            Console.WriteLine("Sterling API ~ Cancel status..." + st + " for order id: " + order_id);
        }

        public void CancellAllSymbol(string symbol, string account)
        {
            structSTICancelAll pStruct = new structSTICancelAll();
            pStruct.bstrSymbol = symbol;
            pStruct.bstrAccount = account;
            Console.WriteLine("Sterling API ~ Cancel All..." + symbol);
            stOrdMaint.CancelAllOrders(ref pStruct);
        }

        public void CancellAll(string account)
        {
            structSTICancelAll pStruct = new structSTICancelAll();
            pStruct.bstrAccount = account;
            stOrdMaint.CancelAllOrders(ref pStruct);
            Console.WriteLine("Sterling API ~ Cancel All...");
        }
        #endregion

        #region Replace orders
        public string ReplaceOrder(string ordID, int qty, double new_price)
        {
            var p = stOrdMaint.GetOrderInfo(ordID);
            structSTIOrder norder = new structSTIOrder();
            
            if (new_price == p.fLmtPrice)
            {
                return "Same_price";
            }

            norder.bstrAccount = p.bstrAccount;
            norder.nPriceType = p.nPriceType;
            norder.nQuantity = qty;
            norder.bstrSymbol = p.bstrSymbol;
            norder.bstrSide = p.bstrSide;
            norder.bstrDestination = p.bstrDestination;
            norder.bstrSide = p.bstrSide;
            norder.fLmtPrice = new_price;
            norder.nDisplay = p.nDisplay;
            norder.bstrTif = p.bstrTif;
            norder.bstrClOrderId = norder.bstrSymbol + norder.bstrSide + norder.bstrDestination + norder.fLmtPrice + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
            var newID = norder.bstrClOrderId;
            int res = stiOrder.ReplaceOrderStruct(norder, 0, ordID);
            if (res != 0)
            {
                Console.WriteLine("replace " + norder.bstrSymbol + " status: " + res);
            }

            string _return = newID + ";" + res;

            return _return;


        }
        #endregion

        #region Positions
        public double Position(string account, string Symbol)
        {
            stiPos.DeRegisterPositions();
            stiEvents.SetOrderEventsAsStructs(true);
            return Convert.ToDouble(stiPos.GetPositionInfo(Symbol, null, account));

        }

        public double GetPositionPrice(string account, string symbol)
        {
            stiPos.DeRegisterPositions();
            Array arrayPos = null;
            stiPos.GetPosListBySym(null, ref arrayPos);
            var smth2 = (structSTIPositionUpdate[])arrayPos;

            for (int i = 0; i < smth2.Count(); i++)
            {
                if (smth2[i].bstrSym.ToString() == symbol)
                {
                    int netPos = (smth2[i].nSharesBot - smth2[i].nSharesSld + smth2[i].nOpeningPosition);
                    double openPrice = Math.Abs(smth2[i].fPositionCost / netPos);
                    return openPrice;
                }

            }

            return 0;
        }

        public string AllPositions(string account)
        {
            stiPos.DeRegisterPositions();
            Array arrayPos = null;
            stiPos.GetPosListBySym(null, ref arrayPos);
            var smth2 = (structSTIPositionUpdate[])arrayPos;
            string pos_list_return = "";
            for (int i = 0; i < smth2.Count(); i++)
            {
                int netPos = (smth2[i].nSharesBot - smth2[i].nSharesSld + smth2[i].nOpeningPosition);
                double openPrice = Math.Abs(smth2[i].fPositionCost / netPos);
                if (smth2[i].bstrAcct == account)
                {
                    pos_list_return += (smth2[i].bstrSym.ToString() + " " + netPos.ToString() + " " + openPrice.ToString() + " " + smth2[i].fReal.ToString() + " " + smth2[i].nOpeningPosition.ToString() + " " + smth2[i].nSharesSldLong + " " + smth2[i].nSharesSldShort + ";");
                }
            }

            return pos_list_return;
        }

        #endregion

        #region OrdersInfo

        public int OrderStatus(string order_id)
        {
            return stOrdMaint_req.GetOrderInfo(order_id).nOrderStatus;
        }


        public int GetOrders()
        {
            int b = stOrdMaint.GetOrderList(true, null);
            return b;
        }
        #endregion


    }
}
