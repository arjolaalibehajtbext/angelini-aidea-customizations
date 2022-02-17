using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Model
{
    public enum ContextMessageName
    {
        NotFound,
        Create,
        Update,
        Delete,
        Retrive,
        Associate,
        Disassociate,
        CopyProductDetailing,
    }

    public enum ContextStageName
    {
        PreValidation = 10,
        PreOperation = 20,
        MainOperation = 30,
        PostOperation = 40
    }

    public enum ContextMode
    {
        Synchronous = 0,
        Asynchronous = 1
    }

    public enum GeographicalLevel
    {
        Country = 108600000,
        State = 108600001,
        Province = 108600002,
        PostalCode = 108600003,
        Brick = 108600004,
        MicroBrick = 108600005
    }

    public enum StateCode
    {
        Active = 0,
        Inactive = 1
    }

    public enum StatusCode
    {
        Active = 1,
        Inactive = 2
    }

    public enum ProductStateCode
    {
        Active = 0,
        Retired = 1,
        Draft = 2,
        UnderRevision = 3
    }

    public enum PlanStatusCode
    {
        Active = 1,
        Inactive = 2,
        Draft = 505480000
    }

    public enum EngagementPlanStatusCode
    {
        Draft = 1,
        Pending = 505480000,
        Approved = 505480001,
        Inactive = 2
    }

    public enum LeadStateCode
    {
        Open = 0,
        Qualified = 1,
        Disqualified = 2
    }

    public enum LeadStatusCode
    {
        New = 1,
        Contacted = 2,
        Qualified = 3,
        Lost = 4,
        CannotContact = 5,
        NoLongerInterested = 6,
        Canceled = 7
    }

    public enum AppointmentStateCode
    {
        Open = 0,
        Completed = 1,
        Canceled = 2,
        Scheduled = 3,
    }

    public enum AppointmentStatusCode
    {
        Free = 1,
        Tentative = 2,
        Completed = 3,
        Canceled = 4,
        Busy = 5,
        OutOfOffice = 6,
    }

    public enum TaskStateCode
    {
        Open = 0,
        Completed = 1,
        Cancelled = 2
    }

    public enum TaskStatusCode
    {
        NotStarted = 2,
        InProgress = 3,
        WaitingOnSomeoneElse = 4,
        Deferred = 7,
        Completed = 5,
        Cancelled = 6,
    }

    public enum DayStatusCode
    {
        Active = 1,
        Inactive = 2,
        Completed = 505480000
    }

    public enum ProductStructure
    {
        Product = 1,
        ProductFamily = 2,
        ProductBundle = 3
    }

    public enum ProductFamilyType
    {
        TherapeuticArea = 505480000,
        Macrobrand = 505480001,
        Brand = 505480002,
    }

    public enum ProductTypeClass
    {
        Sample = 505480000,
        Material = 505480001,
        Detailing = 505480002,
        Erp = 505480003,
    }

    public enum ProductType
    {
        PharmaSample = 1,
        IntegratorSample = 2,
        Detailing = 3,
        Erp = 4,
        ScientificMaterial = 5,
        PromotionalMaterial = 6,
        MedicalDevice = 7
    }

    public enum LimitType
    {
        PerVisit = 505480000,
        Daily = 108600000,
        Weekly = 108600001,
        Monthly = 108600002,
        Quarterly = 108600003,
        Yearly = 108600004,
    }

    public enum SecurityRole
    {
        SuperHq = 108600000,
        Hq = 108600001,
        Sam = 108600002,
        Am = 108600003,
        Rep = 108600004
    }

    public enum ActivityClass
    {
        Generic = 505480000,
        Tutoring = 505480001,
        Visit = 505480002
    }

    public enum ContactType
    {
        Doctor = 108600000,
        Stakeholder = 108600001
    }

    public enum AppointmentTypeClass
    {
        MedInfoCall = 505480000,
        Contact = 505480001,
    }

    public enum ContactSensitiveFields
    {
        ContactType = 505480000,
        GenderCode = 505480001,
        LastName = 505480002,
        MiddleName = 505480003,
        FirstName = 505480004,
        GovernmentId = 505480005,
        BirthDate = 505480006,
        EmailAddress1 = 505480007,
        tb_specializationid1 = 505480008,
        tb_specializationid2 = 505480008,
        tb_specializationid3 = 505480008,
        tb_specializationid4 = 505480008,
        tb_specializationid5 = 505480008,
        roleonaccount = 505480009
    }

    public enum AccountTypeClass
    {
        Ambulatory = 505480000,
        Structure = 505480001,
        Distributor = 505480002,
        Pharmacy = 505480003,
        Section = 505480004,
    }
    public enum TagModule
    {
        CLM = 505480000,
        RTE = 505480001,
    }


    public enum RteStatusCode
    {
        Active = 1,
        Inactive = 2,
        Draft = 505480000
    }

    public enum ValidationStatus
    {
        ToBeValidated = 108600000,
        Validating = 108600001,
        Validated = 108600002,
        Rejected = 108600003,
        ToBeRevalidated = 108600004,
        OutOfValidation = 108600005,
    }

    public enum ValidationStatusConfigRole
    {
        HQ = 505480000,
        NonHQ = 505480001
    }

    public enum ValidationRegaring
    {
    }

    public class SupportLists
    {
        public static IEnumerable<dynamic> contactSensitiveFields()
        {
            return new List<dynamic>{
                new { Name = "tb_contacttype", Code = 505480000 },
                new { Name = "gendercode", Code = 505480001 },
                new { Name = "lastname", Code = 505480002 },
                new { Name = "middlename", Code = 505480003 },
                new { Name = "firstname", Code = 505480004 },
                new { Name = "governmentid", Code = 505480005 },
                new { Name = "birthdate", Code = 505480006 },
                new { Name = "emailaddress1", Code = 505480007 },
                new { Name = "tb_specializationid1", Code = 505480008 },
                new { Name = "tb_specializationid2", Code = 505480008 },
                new { Name = "tb_specializationid3", Code = 505480008 },
                new { Name = "tb_specializationid4", Code = 505480008 },
                new { Name = "tb_specializationid5", Code = 505480008 },
                new { Name = "roleonaccount", Code = 505480009 },
            };
        }

        public static IEnumerable<dynamic> accountSensitiveFields()
        {
            return new List<dynamic>{
                new { Name = "parentaccountid", Code = 505480000 },
                new { Name = "tb_richname", Code = 505480001 },
                new { Name = "accounttype", Code = 505480002 },
                new { Name = "emailaddress1", Code = 505480003 },
                new { Name = "telephone1", Code = 505480004 },
                new { Name = "fax", Code = 505480005 },
                new { Name = "tb_street", Code = 505480006 },
                new { Name = "tb_town", Code = 505480007 },
                new { Name = "tb_postalcode", Code = 505480008 },
                new { Name = "tb_province", Code = 505480009 },
                new { Name = "tb_brick", Code = 505480010 },
                new { Name = "tb_microbrick", Code = 505480011 }
            };
        }
    }
}
