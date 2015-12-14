using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace Core
{
    public class LdapList
    {
        int limit = 4000;

        public List<Users> GetADUsersWrapper(string DomainPath, ref List<Users> DiscardedUsers)
        {
            return GetADUsers(DomainPath, ref DiscardedUsers, null, null);
        }

        // DomainPath = LDAP://AD_Server
        public List<Users> GetADUsersWrapper(string DomainPath, ref List<Users> DiscardedUsers, string Username, string Password)
        {
            List<Users> lstADUsers = new List<Users>();

            if (DomainPath.ToLower().IndexOf("ou=") == -1)
            {
                //search.Filter = "(&(objectClass=person))";

                lstADUsers = GetADUsers(DomainPath, ref DiscardedUsers, Username, Password);

            }
            else
            {
                // PROCURAR DENTRO DE UMA OU
                DirectoryEntry searchRoot = null;

                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                {
                    searchRoot = new DirectoryEntry(DomainPath, Username, Password, AuthenticationTypes.None);
                }
                else
                {
                    searchRoot = new DirectoryEntry(DomainPath);
                }

                DirectorySearcher search = new DirectorySearcher(searchRoot);
                search.ReferralChasing = ReferralChasingOption.All;

                search.SizeLimit = limit;
                search.PageSize = limit;

                search.SearchScope = SearchScope.Subtree;

                search.Filter = "(&(objectClass=groupOfUniqueNames))";

                search.PropertiesToLoad.Add("uniqueMember");

                SearchResultCollection resultCol = search.FindAll();

                if (resultCol != null) {
                    for (int counter = 0; counter < resultCol.Count; counter++)
                    {
                        if (resultCol[counter].Properties.Contains("uniqueMember")) {
                            System.DirectoryServices.ResultPropertyValueCollection res = resultCol[counter].Properties["uniqueMember"];

                            for (int userCount = 0; userCount < res.Count; userCount++)
                            {
                                string userDN = null;
                                if (res[userCount] != null)
                                {
                                    userDN = (string)res[userCount];
                                    //DirectoryEntry de = new DirectoryEntry(userDN, Username, Password);

                                    string newDomainPath = null;

                                    if (DomainPath.LastIndexOf("/") > -1 && DomainPath.Length > DomainPath.LastIndexOf("/")+1)
                                    {
                                        newDomainPath = DomainPath.Substring(0, DomainPath.LastIndexOf("/"));
                                        newDomainPath = newDomainPath + "/" + userDN;
                                    }

                                    if (!string.IsNullOrEmpty(newDomainPath))
                                    {
                                        List<Users> singleUser = new List<Users>();
                                        singleUser = GetADUsers(newDomainPath, ref DiscardedUsers, Username, Password);

                                        lstADUsers.AddRange(singleUser);
                                    }
                                }
                            }
                            

                        }
                    }
                }

                //lstADUsers = LoadUsers(resultCol, ref DiscardedUsers);

                /*
                SearchResult subOU = search.FindOne();

                DirectoryEntry mySubOU = subOU.GetDirectoryEntry();

                if (mySubOU != null)
                {
                    lstADUsers = GetADUsers(DomainPath, ref DiscardedUsers, Username, Password, mySubOU);
                }
                */
                
            }

            return lstADUsers;
        }

        public List<Users> GetADUsers(string DomainPath, ref List<Users> DiscardedUsers, string Username, string Password)
        {
            

            List<Users> lstADUsers = new List<Users>();

            if (DiscardedUsers == null)
            {
                DiscardedUsers = new List<Users>();
            }

            DirectoryEntry SearchRoot = null;

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                SearchRoot = new DirectoryEntry(DomainPath, Username, Password, AuthenticationTypes.None);
            }
            else
            {
                SearchRoot = new DirectoryEntry(DomainPath);
            }

            DirectorySearcher search = new DirectorySearcher(SearchRoot);
            search.ReferralChasing = ReferralChasingOption.All;

            search.SizeLimit = limit;
            search.PageSize = limit;

            search.SearchScope = SearchScope.Subtree;

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                // OpenLDAP
                search.Filter = "(&(objectClass=person))";
            }
            else
            {
                // ACTIVE DIRECTORY
                search.Filter = "(&(objectClass=user)(objectCategory=person))";
            }


            search.PropertiesToLoad.Add("name");
            search.PropertiesToLoad.Add("canonicalname");
            search.PropertiesToLoad.Add("givenname");
            search.PropertiesToLoad.Add("userprincipalname");
            search.PropertiesToLoad.Add("uid");

            search.PropertiesToLoad.Add("samaccountname");
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("usergroup");
            search.PropertiesToLoad.Add("displayname");//first name
            search.PropertiesToLoad.Add("useraccountcontrol");//first name
            search.PropertiesToLoad.Add("dn");//first name
            search.PropertiesToLoad.Add("uniqueMember");

            SearchResultCollection resultCol = search.FindAll();

            lstADUsers = LoadUsers(resultCol, ref DiscardedUsers);
            

            return lstADUsers;
        }

        public List<Users> LoadUsers(SearchResultCollection ResultCollection, ref List<Users> DiscardedUsers)
        {
            SearchResult result;
            List<Users> loadedUsers = new List<Users>();

            if (ResultCollection != null)
            {
                for (int counter = 0; counter < ResultCollection.Count; counter++)
                {
                    string UserNameEmailString = string.Empty;
                    result = ResultCollection[counter];
                    if ( (result.Properties.Contains("samaccountname") && result.Properties.Contains("displayname")) || 
                         (result.Properties.Contains("uid"))
                        )
                    {
                        Users objSurveyUsers = new Users();
                        if (result.Properties.Contains("mail"))
                        {
                            objSurveyUsers.Email = (string)result.Properties["mail"][0];
                        }
                        if (result.Properties.Contains("useraccountcontrol"))
                        {
                            objSurveyUsers.UserAccountControl = (UserAccountControl)result.Properties["useraccountcontrol"][0];
                        }
                        if (result.Properties.Contains("samaccountname"))
                        {
                            objSurveyUsers.UserName = (string)result.Properties["samaccountname"][0];
                        }
                        if (result.Properties.Contains("displayname"))
                        {
                            objSurveyUsers.DisplayName = (string)result.Properties["displayname"][0];
                        }

                        // novas propriedades
                        if (result.Properties.Contains("name"))
                        {
                            objSurveyUsers.Name = (string)result.Properties["name"][0];
                        }
                        if (result.Properties.Contains("givenname"))
                        {
                            objSurveyUsers.GivenName = (string)result.Properties["givenname"][0];
                        }
                        if (result.Properties.Contains("userprincipalname"))
                        {
                            objSurveyUsers.UserPrincipalName = (string)result.Properties["userprincipalname"][0];
                        }
                        if (result.Properties.Contains("canonicalname"))
                        {
                            objSurveyUsers.CanonicalName = (string)result.Properties["canonicalname"][0];
                        }
                        if (result.Properties.Contains("uid"))
                        {
                            objSurveyUsers.Uid = (string)result.Properties["uid"][0];
                        }
                        if (result.Properties.Contains("dn"))
                        {
                            objSurveyUsers.DistinguishedName = (string)result.Properties["dn"][0];
                        }

                        loadedUsers.Add(objSurveyUsers);
                    }
                    else
                    {
                        // usuario nao possui as propriedades SAM-Account-Name e Display Name
                        Users objectsDiscarded = new Users();
                        if (result.Properties.Contains("mail"))
                        {
                            objectsDiscarded.Email = (string)result.Properties["mail"][0];
                        }
                        if (result.Properties.Contains("useraccountcontrol"))
                        {
                            objectsDiscarded.UserAccountControl = (UserAccountControl)result.Properties["useraccountcontrol"][0];
                        }

                        if (result.Properties.Contains("samaccountname"))
                        {
                            objectsDiscarded.UserName = (string)result.Properties["samaccountname"][0];
                        }
                        if (result.Properties.Contains("displayname"))
                        {
                            objectsDiscarded.DisplayName = (string)result.Properties["displayname"][0];
                        }

                        // novas propriedades
                        if (result.Properties.Contains("name"))
                        {
                            objectsDiscarded.Name = (string)result.Properties["name"][0];
                        }
                        if (result.Properties.Contains("givenname"))
                        {
                            objectsDiscarded.GivenName = (string)result.Properties["givenname"][0];
                        }
                        if (result.Properties.Contains("userprincipalname"))
                        {
                            objectsDiscarded.UserPrincipalName = (string)result.Properties["userprincipalname"][0];
                        }
                        if (result.Properties.Contains("canonicalname"))
                        {
                            objectsDiscarded.CanonicalName = (string)result.Properties["canonicalname"][0];
                        }
                        if (result.Properties.Contains("uid"))
                        {
                            objectsDiscarded.Uid = (string)result.Properties["uid"][0];
                        }
                        if (result.Properties.Contains("dn"))
                        {
                            objectsDiscarded.DistinguishedName = (string)result.Properties["dn"][0];
                        }

                        DiscardedUsers.Add(objectsDiscarded);
                    }
                }
            }

            return loadedUsers;
        }

    }

}
