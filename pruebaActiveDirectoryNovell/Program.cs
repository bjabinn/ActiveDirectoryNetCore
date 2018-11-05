using System;
using Novell.Directory.Ldap;

namespace pruebaActiveDirectory
{
    static class Constants {
        public const String ldapHost="10.125.8.21";
    }
    class Program
    {
        private int checkUser(String loginDN, String password) {
            // Metemos los valores de configuración para conectarnos al ldap de Everis.
           int LdapPort = LdapConnection.DEFAULT_PORT;
           //int searchScope = LdapConnection.SCOPE_ONE;
           int LdapVersion = LdapConnection.Ldap_V3;
           //bool attributeOnly=true;
           String[] attrs = {LdapConnection.NO_ATTRS};
           LdapConnection lc = new LdapConnection();
           int resultado=0;
           // Vamos a meter una restricción de tiempo.
           LdapSearchConstraints constraints = new LdapSearchConstraints();
           constraints.TimeLimit=10000; // ms
           try{
                // Nos conectamos al servidor.
                lc.Connect(Constants.ldapHost,LdapPort);
                // Accedemos con las credenciales del usuario para ver si está.
                lc.Bind(LdapVersion,loginDN,password);
                lc.Disconnect();
           } catch (LdapException e) {
                   resultado=e.ResultCode;
           } catch (Exception) { 
               resultado=-1;
           }
           return resultado;
        }
        static void Main(string[] args)
        {
           Program p = new Program();
           String loginDN="Usersad\\jmunozga";
           String password="Temporal14";
           Console.WriteLine(p.checkUser(loginDN,password));
        }        
    }
}
