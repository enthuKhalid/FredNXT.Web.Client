using System;
using System.Collections.Generic;

namespace FredNXT.Web.Client
{
    #region PO details class
    public class PODetails
    {
        public POHeader Header { get; set; }
        public List<POLine> Lines { get; set; }

        /// <summary>
        /// Generates a dummy input Purchase order details object
        /// </summary>
        public PODetails()
        {
            Header = new POHeader()
            {
                CurrencyCode = "AUD",
                InventLocationId = "000001",
                InventSiteId = "000001",
                PurchaseTypeValue = "Purch",
                PurchStatusValue = "Backorder",
                VendAccount = "SIG00001"
            };
            Lines =
                new List<POLine>()
                {
                    new POLine()
                    {
                        CurrencyCode = "AUD",
                        InventLocationId = "000001",
                        InventSiteId = "000001",
                        ItemId = "100895",
                        PurchPrice = 25,
                        PurchQty = 10,
                        PurchUnit = "ea"
                    },
                    new POLine()
                    {
                        CurrencyCode = "AUD",
                        InventLocationId = "000001",
                        InventSiteId = "000001",
                        ItemId = "106397",
                        PurchPrice = 15,
                        PurchQty = 100,
                        PurchUnit = "ea"
                    }
                };
        }

    }
    #endregion

    #region enums
    public enum PurchaseType
    {
        Journal = 0,
        Quotation = 1,
        Subscription = 2,
        Purch = 3,
        ReturnItem = 4
    }

    public enum PurchaseStatus
    {
        None = 0,
        Backorder = 1,
        Received = 2,
        Invoiced = 3,
        Cancelled = 4
    }
    #endregion

    #region POHeader and line class definitions
    public class POHeader
    {
        public long RecId { get; set; }
        public string PurchId { get; set; }
        public string VendAccount { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string CurrencyCode { get; set; }
        public string InventLocationId { get; set; }
        public string InventSiteId { get; set; }
        public PurchaseType PurchaseType { get; set; }
        public PurchaseStatus PurchStatus { get; set; }

        public string PurchaseTypeValue
        {
            get { return this.PurchaseType.ToString(); }
            set
            {
                PurchaseType purchaseType;
                PurchaseType.TryParse(value, true, out purchaseType);
                this.PurchaseType = purchaseType;
            }
        }

        public string PurchStatusValue
        {
            get { return this.PurchStatus.ToString(); }
            set
            {
                PurchaseStatus purchaseOrderStatus;
                PurchaseStatus.TryParse(value, true, out purchaseOrderStatus);
                this.PurchStatus = purchaseOrderStatus;
            }
        }
    }

    public class POLine
    {
        public long RecId { get; set; }
        public string PurchId { get; set; }
        public string CurrencyCode { get; set; }
        public string InventLocationId { get; set; }
        public string InventSiteId { get; set; }
        public string ItemId { get; set; }
        public decimal PurchPrice { get; set; }
        public decimal PurchQty { get; set; }
        public string PurchUnit { get; set; }
    }
    #endregion
}
