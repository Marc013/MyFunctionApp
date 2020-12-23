Connect-AzureAD -TenantId $TenantID
$DisplayNameOfMSI = 'MyAadFunction'
$MSI = (Get-AzureADServicePrincipal -Filter "displayName eq '$DisplayNameOfMSI'")
$MSGraphAppId = "00000003-0000-0000-c000-000000000000" #Microsoft Graph aka graph.microsoft.com
$GraphServicePrincipal = Get-AzureADServicePrincipal -Filter "appId eq '$MSGraphAppId'"
$PermissionName = "Directory.Read.All"
$AppRole = $GraphServicePrincipal.AppRoles | Where-Object {$_.Value -eq $PermissionName -and $_.AllowedMemberTypes -contains "Application"}
New-AzureAdServiceAppRoleAssignment -ObjectId $MSI.ObjectId -PrincipalId $MSI.ObjectId -ResourceId $GraphServicePrincipal.ObjectId -Id $AppRole.Id

#NOTE: The above assignment may indicate bad request or indicate failure but it has been noted that the permission assignment still succeeds and you can verify with the following command

Get-AzureADServiceAppRoleAssignment -ObjectId $GraphServicePrincipal.ObjectId | Where-Object {$_.PrincipalDisplayName -eq $DisplayNameOfMSI} | Format-List -Property *
