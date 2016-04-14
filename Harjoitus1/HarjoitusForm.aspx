<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HarjoitusForm.aspx.cs" Inherits="Harjoitus1.HarjoitusForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Juuso Tenhunen Harjoitustehtävä</title>
    <link rel="stylesheet" type="text/css" href="style/style.css" title="style" />
</head>
<body>
    <div id="main">
        <div id="header">
            <div id="logo">
                <div id="logo_text">
                    <!-- class="logo_colour", allows you to change the colour of the text -->
                    <h1><a href="HarjoitusForm.aspx">Harjoitus<span class="logo_colour">tehtävä</span></a></h1>
                    <h2>Juuso Tenhunen</h2>
                </div>
            </div>
            <div id="menubar">
                <ul id="menu">
                    <!-- put class="selected" in the li tag for the selected page - to highlight which page you're on -->
                    <li class="selected"><a href="HarjoitusForm.aspx">Etusivu</a></li>
                </ul>
            </div>
        </div>
        <div id="site_content">
            <div class="sidebar">
                <!-- insert your sidebar items here -->
                <h3>Etsi otteluita</h3>
                <form method="post" action="#" id="search_form" runat="server">
                    <div>
                        <asp:TextBox ID="EkaPaiva" placeholder="Mistä päivästä pv pp.kk.vvvv" runat="server"></asp:TextBox>
                        <asp:TextBox ID="TokaPaiva" placeholder="Mihin päivään pv pp.kk.vvvv" runat="server"></asp:TextBox>
                        <label></label><asp:TextBox ID="Joukkue" placeholder="Voit myös hakea joukkuetta" runat="server"></asp:TextBox>
                        <br />
                        <p></p>
                        <asp:ImageButton ID="HaeButton" runat="server" ImageUrl="~/style/search.png" OnClick="HaeButton_Click" />
                    </div>
                </form>
            </div>
            <div id="content">
                <!-- insert the page content here -->
                <h1>Ottelulista</h1>
                
                <asp:Label ID="Varoitus" runat="server"></asp:Label>
                <div id="TulosDiv" runat="server">
                </div>

            </div>
        </div>
        <div id="footer">
            Copyright &copy; black_white | <a href="http://validator.w3.org/check?uri=referer">HTML5</a> | <a href="http://jigsaw.w3.org/css-validator/check/referer">CSS</a> | <a href="http://www.html5webtemplates.co.uk">Free CSS Templates</a>
        </div>
    </div>
</body>
</html>
