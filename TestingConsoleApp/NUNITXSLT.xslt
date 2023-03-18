<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:html="http://www.w3.org/TR/REC-html40"
  exclude-result-prefixes="html">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/test-run">
		<html>
			<head>
				<title>Test Run Summary Pie Chart</title>
				<script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>
				<script type="text/javascript">
					google.charts.load('current', {'packages':['corechart']});
					google.charts.setOnLoadCallback(drawChart);

					function drawChart() {
					var data = google.visualization.arrayToDataTable([
					['Result', 'Count'],
					['Passed', <xsl:value-of select="@passed"/>],
					['Failed', <xsl:value-of select="@failed"/>],
					['Warnings', <xsl:value-of select="@warnings"/>],
					['Inconclusive', <xsl:value-of select="@inconclusive"/>],
					['Skipped', <xsl:value-of select="@skipped"/>]
					]);

					var options = {
					title: 'Test Run Summary',
					pieHole: 0.4,
					colors: ['#00FF00', '#FF0000', '#FFA500', '#808080', '#FFFF00']
					};

					var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
					chart.draw(data, options);
					}
				</script>
			</head>
			<body>
				<div id="chart_div" style="width: 900px; height: 500px;"></div>

				<h4>Command Line</h4>
				<pre>
					<xsl:value-of select="command-line"/>
				</pre>

			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>