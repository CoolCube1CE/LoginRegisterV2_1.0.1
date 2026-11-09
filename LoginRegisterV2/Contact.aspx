<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="LoginRegisterV2.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="content-page" aria-labelledby="title">
        <div class="card">
            <h2 id="title"><%: Title %></h2>
            <h3 class="text-muted mt-0">Your contact page.</h3>
            <address>
                One Microsoft Way<br />
                Redmond, WA 98052-6399<br />
                <abbr title="Phone">P:</abbr>
                425.555.0100
            </address>

            <address>
                <strong>Support:</strong> <a href="mailto:Support@example.com">Support@example.com</a><br />
                <strong>Marketing:</strong> <a href="mailto:Marketing@example.com">Marketing@example.com</a>
            </address>
        </div>
    </main>
</asp:Content>
