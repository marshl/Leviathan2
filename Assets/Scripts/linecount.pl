use utf8;
use 5.012;

use strict;
use warnings;

use File::Find::Rule;

my @FileList = File::Find::Rule->file->name('*.cs')->in(".");

my $lineCount = 0;
foreach my $file ( @FileList )
{
	open my $fh, "<", $file or die $!;
	
	while ( <$fh> )
	{
		++$lineCount;
	}
}

print "$lineCount lines in " . scalar @FileList . " files";

#PS ls -inc "*.cs" -Recurse | %{cat $_} | Measure-Object