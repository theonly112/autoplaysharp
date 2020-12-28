Get-ChildItem * -Include *.pdb -Recurse | Remove-Item

$dirs = $data = @('dll','x86','cs','de','es','fr','it','ja','ko','pl','pt-BR','ru','tr','zh-Hans','zh-Hant')
foreach($d in $dirs)
{
	Remove-Item .\App.Wpf\bin\Release\$d -Recurse -ErrorAction SilentlyContinue
}
