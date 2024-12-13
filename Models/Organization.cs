using Microsoft.Identity.Client;

namespace coreapi.Models
{
    public class OrganizationTeam
    {
        public string Id { get; set; }
        public string UpdUser { get; set; }
        public string TeamName { get; set; }
        public int JobDescription { get; set; }
        public string CmpId { get; set; }
        public string Approvedocby { get; set; }
        public string TeamAll { get; set; }
    }

    public class Organization
    {
        public string Id { get; set; }
        public string Position { get; set; }
        public int AccountID { get; set; }
        public string FullName { get; set; }
        public string ImgPath { get; set; }
        public int StateApprove { get; set; }
        public string CmpId { get; set; }
        public List<Organization> children { get; set; }
    }

    public class OrganizationAction
    {
        public string Id { get; set; }
        public string CmpId { get; set; }
        public int AccountID { get; set; }
        public int ParrentID { get; set; }
        public int StateApprove { get; set; }
        public string UpdUser { get; set; }
        public string Position { get; set; }
    }

    public class OrganizationTable
    {
        public string Id { get; set; }
        public string Position { get; set; }
        public int AccountID { get; set; }
        public int ParrentID { get; set; }
        public string FullName { get; set; }
        public string ImgPath { get; set; }
        public int StateApprove { get; set; }
        public string CmpId { get; set; }
    }
}
