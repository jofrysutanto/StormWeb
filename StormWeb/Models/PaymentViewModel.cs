﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StormWeb.Models
{
    public class PaymentViewModel
    {
            public List<Application> unpaidApplications { get; set; }
            public List<Application> paidApplications { get; set; }
            public List<Payment> paymentTable { get; set; }
            public List<Receipt_File> receiptTable { get; set; }

    }
}