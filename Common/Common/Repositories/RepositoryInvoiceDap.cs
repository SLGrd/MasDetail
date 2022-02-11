using Common.Models;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using static Common.Glb;

namespace Common.Repositories;
public class InvoiceCrud
{
    public static Invoice SaveInvoice(Invoice invoice)
    {
        try
        {
            using IDbConnection cn = new SqlConnection(CnnString);
            using (var transaction = new TransactionScope())
            {
                string sqlHeader = 
                          "DECLARE @NextInvNumber int ; " 
                        + "SET @NextInvNumber = NEXT VALUE FOR Hrz.EventCounter; "
                        + "Insert into InvHeader "
                        + "(  InvoiceNumber,    dtInvoice,  Buyer,  Cpf,  Address,  Complto,  Phone,  Gid)" + " values "
                        + "( @NextInvNumber, @InvoiceDate, @Buyer, @Cpf, @Address, @Complto, @Phone, @Gid)";

                string sqlInvoice = 
                          "Insert into InvLines "
                        + "( ItemNumber,  Description,  Qtty,  UnitPrice,  Gid)" + " values "
                        + "(@ItemNumber, @Description, @Qtty, @UnitPrice, @Gid)";
                cn.Open();

                var affectedRows = cn.Execute(sqlHeader, new
                {                        
                    invoice.Header.InvoiceDate,
                    invoice.Header.Buyer,
                    invoice.Header.Cpf,
                    invoice.Header.Address,
                    invoice.Header.Complto,
                    invoice.Header.Phone,
                    invoice.Header.Gid
                });

                //throw new Exception($"Erro gerado para efeito de testes : {DateTime.Now:dd-MM-yyyy hh:mm:ss}");

                for (int n = 0; n < invoice.Lines!.Count; n++)
                    affectedRows = cn.Execute(sqlInvoice, new
                    {
                        invoice.Lines[n].ItemNumber,
                        invoice.Lines[n].Description,
                        invoice.Lines[n].Qtty,
                        invoice.Lines[n].UnitPrice,
                        invoice.Lines[n].Gid
                    });         

                //throw new Exception($"Erro gerado para efeito de testes : {DateTime.Now:dd-MM-yyyy hh:mm:ss}");
                //  Deu tudo certo
                transaction.Complete();
            }
            return invoice;
        }
        catch (Exception)
        {
            throw;  //  Re trhow exception to be treated where we have user interface 
        }
    }

    public static void DeleteAll()
    {
        try
        {
            using IDbConnection cn = new SqlConnection(CnnString);
            cn.Open();
            cn.Execute("Delete from InvHeader");
        }
        catch (Exception)
        {
            throw;  //  Re trhow exception to be treated where we have user interface 
        }
    }
}