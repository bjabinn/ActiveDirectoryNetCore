using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;

namespace pruebaActiveDirectory2
{
    class Program
    {
        
        public bool checkUser(String loginDn,String password){
            bool res=false;
            DirectoryEntry entry = new DirectoryEntry();
            entry.Username=loginDn;
            entry.Password=password;
            DirectorySearcher searcher = new DirectorySearcher(entry);
            searcher.Filter = "(&(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
            try{
                searcher.FindOne();
                res=true;
            }catch (COMException) {
                /* 
                // Autentication Error.
                if (ex.ErrorCode == -2147023570) {
                    res=false;
                } */
            }
            return res;
        }
        public void getDirectorySearcher(String stringSearch, int pageSize=5){
            DirectorySearcher searcher = new DirectorySearcher();
            searcher.SizeLimit=pageSize;
            searcher.ClientTimeout=TimeSpan.FromSeconds(-1);
            searcher.Filter = string.Format("(&(objectCategory=person)(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2))(|(DisplayName=*{0}*)(SamAccountName=*{0}*)))",stringSearch,true);
            searcher.PropertiesToLoad.Add("SamAccountName");
            searcher.PropertiesToLoad.Add("thumbnailPhoto");
            using (SearchResultCollection results = searcher.FindAll()) {
                foreach (SearchResult domainUsers in results) {
                    DirectoryEntry user = domainUsers.GetDirectoryEntry();
                    if(user!=null){
                        user.RefreshCache(new String[]{"SamAccountName"});
                        String samAccountName= user.Properties["SamAccountName"][0].ToString();
                        var photoP=user.Properties["thumbnailPhoto"].Value;
                        if(photoP!=null){
                            byte[] photoInBytes=(byte[])photoP;
                            Bitmap photo = new Bitmap(new MemoryStream(photoInBytes)); 
                            photo.Save(String.Format("./fotos1/{0}.jpg",samAccountName),ImageFormat.Jpeg);
                        }
                    }
                }
            }
        }
        public void getUsersPrincipal(String stringSearch, int pageSize=5)
        {
            using (var ctx= new PrincipalContext(ContextType.Domain)){
                var myDomainUsers = new List<string>();
                List <UserPrincipal> searchPrinciples = new List<UserPrincipal>();
                searchPrinciples.Add(new UserPrincipal(ctx){DisplayName= String.Format("*{0}*",stringSearch), Enabled=true});
                searchPrinciples.Add(new UserPrincipal(ctx){SamAccountName=String.Format("*{0}*",stringSearch), Enabled=true});
                foreach (UserPrincipal item in searchPrinciples) {
                    PrincipalSearcher search = new PrincipalSearcher(item);
                    foreach(var domainUsers in search.FindAll()){ 
                        pageSize--;                     
                        String samAccountName= domainUsers.SamAccountName;
                        DirectoryEntry directoryEntry = (DirectoryEntry)domainUsers.GetUnderlyingObject();
                        PropertyValueCollection photoProperty=directoryEntry.Properties["thumbnailPhoto"];
                        if(photoProperty.Value!=null && photoProperty.Value is byte[]) {
                            byte[] photoInBytes = (byte [])photoProperty.Value;
                            Bitmap photo = new Bitmap(new MemoryStream(photoInBytes)); 
                            photo.Save(String.Format("./fotos/{0}.jpg",samAccountName),ImageFormat.Jpeg);
                        }
                        if(pageSize==0)
                            return;  
                    }
                }   
            }
        }
        static void Main(string[] args)
        {
            Program p = new Program();
           //p.checkUser("Usersad\\jmunozga","Temporal14");
            
            long sumtimerDirectorySearch=0;
            long sumtimerUserPrincipal=0;
            for(int i=0;i<10;i++) {
                Stopwatch timer = Stopwatch.StartNew();
                p.getDirectorySearcher("munoz",5);
                timer.Stop();
                sumtimerDirectorySearch+=timer.ElapsedMilliseconds;
                timer = Stopwatch.StartNew();
                p.getUsersPrincipal("munoz",5);
                timer.Stop();
                sumtimerUserPrincipal+=timer.ElapsedMilliseconds;
            }
            Console.WriteLine("DirectorySearch {0} ms", sumtimerDirectorySearch/10);
            Console.WriteLine("UsersPrincipal {0} ms", sumtimerUserPrincipal/10);
        }
    }
}
