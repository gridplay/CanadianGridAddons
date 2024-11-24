# Canadian Grid Addons For OpenSim community

## Abuse Reports
Compile the DLL with opensim for dotnet and drop into your region bin folder.
In OpenSim.ini do this near the bottom
```ini
[AbuseReports]
    Enabled = true
    ABURL = https://yoursite.whatever/abusereports
    AbuseReportModule = AbuseReports
```
Data will be sent as query string in the POST method

## Premium
Kinda useless without modding the OpenSim.Service.LLLoginService/LLLoginResponse.cs file

This DLL is for the Robust system.

Line 473 ish
```cs
int prem = IPremium.GetPrem(account.PrincipalID);
if (prem == 1)
{
	MaxAgentGroups = (Constants.MaxAgentGroups * 2)
}else if (prem == 2)
{
	MaxAgentGroups = (Constants.MaxAgentGroups * 4)
}
```
Or put whatever value you want where 1 is prem and 2 is prem+. On the Canadian Grid, its 50 * 2 or else 50 * 4

Some more coding required to initate the IPremium interface.

No custom INI, just the DATABASE connect string.

### MySQL
`uuid` is the PrinicalID from UserAccounts

`prem` is a int of premium rank like 1 for prem, 2 for prem+

`expire` is when the user's prem/prem+ expires.


More addons will come soon.

## Credits
Chris Strachan of GridPlay Productions aka Ven
