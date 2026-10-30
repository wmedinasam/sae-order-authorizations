using ORD_SAE.MODEL;
using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace ORD_SAE.SERVICE
{
    class MailService
    {
        private static SystemConfugurationModel systemConfigurationModel = new SystemConfugurationModel();

        public static ResponseModel<object> SendMail(string Orden, string isAproved, string empresa, string email, string motivo = "")
        {
            ResponseModel<object> res = new ResponseModel<object>();
            UserModel _userModel = new UserModel();
            try
            {
                var responseSystemConfiguration = systemConfigurationModel.GetConfiguration();
                if (responseSystemConfiguration.Success)
                {
                    var response = _userModel.GetUser();
                    if (response.Success)
                    {
                        var result = response.Object.Where(x => x.SendMail);
                        if (result.Count() != 0)
                        {
                            foreach (var item in result)
                            {
                                if (!item.EsAdministrador) // Agregar la condición de a quien enviar el correo.
                                {
                                    if (item.Rol != "OC" && item.Rol != "CONTA")
                                    {
                                        string mailFrom = responseSystemConfiguration.Object.Where(x => x.Key == "MailFrom").FirstOrDefault().Value;
                                        string mailFromPassword = responseSystemConfiguration.Object.Where(x => x.Key == "MailPassword").FirstOrDefault().Value;
                                        string subject = responseSystemConfiguration.Object.Where(x => x.Key == "MailSubject").FirstOrDefault().Value.DesencriptarDatos();
                                        string body = responseSystemConfiguration.Object.Where(x => x.Key == "MailBody").FirstOrDefault().Value.DesencriptarDatos();
                                        if (!String.IsNullOrEmpty(motivo))
                                            body = String.Concat(body, " Por: ", motivo);
                                        MailMessage mail = new MailMessage();
                                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                                        mail.From = new MailAddress(mailFrom);
                                        mail.To.Add(item.Mail);
                                        if (!String.IsNullOrEmpty(email))
                                            mail.CC.Add(email);
                                        var otherMailCopy = response.Object.Where(x => x.Rol == "OC");
                                        var accountingCopy = response.Object.Where(x => x.Rol == "CONTA");
                                        string mailAddressForCC = string.Empty;
                                        string mailAddressForCCAcoounting = string.Empty;
                                        if (otherMailCopy.Count() != 0)
                                        {
                                            mailAddressForCC = otherMailCopy.First().Mail;
                                            mail.CC.Add(mailAddressForCC);
                                            if (!String.IsNullOrEmpty(accountingCopy.First().Mail))
                                            {
                                                mailAddressForCCAcoounting = accountingCopy.First().Mail;
                                                mail.CC.Add(mailAddressForCCAcoounting);
                                            }
                                        }
                                        mail.Subject = string.Format(subject, item.FullName, Orden, isAproved, empresa);
                                        mail.Body = string.Format(body, item.FullName, Orden, isAproved, empresa);
                                        SmtpServer.Port = 587;
                                        SmtpServer.Credentials = new System.Net.NetworkCredential(mailFrom, mailFromPassword);
                                        SmtpServer.EnableSsl = true;
                                        SmtpServer.Send(mail);
                                        res.Success = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            res.Success = false;
                            res.Message = "No hay usuarios con correo disponibles";
                        }
                    }
                    else
                    {
                        res.Success = false;
                        res.Message = response.Message;
                    }

                }
                else
                {
                    res.Success = false;
                    res.Message = responseSystemConfiguration.Message;
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Message = ex.Message;
            }
            return res;
        }
    }
}
