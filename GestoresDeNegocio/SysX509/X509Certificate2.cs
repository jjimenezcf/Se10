namespace SysX509
{
    internal class X509Certificate2
    {
        private string v;
        private string password;

        public X509Certificate2(string v, string password)
        {
            this.v = v;
            this.password = password;
        }
    }
}