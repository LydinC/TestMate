<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="yes" />

	<xsl:template match="/">
		<html>
			<head>
				<title>NUnit Test Results</title>
				<style type="text/css">
					body { font-family: Arial, sans-serif; font-size: 12px; }
					h1 { font-size: 18px; font-weight: bold; text-align: center; }
					table { border-collapse: collapse; width: 100%; }
					th { background-color: #666; color: #fff; font-weight: bold; text-align: left; padding: 5px; }
					td { border: 1px solid #ccc; padding: 5px; }
					.pass { background-color: #9f9; }
					.fail { background-color: #f99; }
					.warning { background-color: #ff0; }
					.inconclusive { background-color: #ddd; }
					.skipped { background-color: #eee; }
					.duration { font-weight: bold; }
				</style>
			</head>
			<body>
				<xsl:apply-templates select="//test-run" />
			</body>
		</html>
	</xsl:template>

	<xsl:template match="test-run">
		<h1>NUnit Test Results</h1>
		<p>
			<span class="duration">Duration:</span> <xsl:value-of select="@duration" /> seconds
		</p>
		<p>
			<span class="status">Status:</span>
			<xsl:value-of select="@result" />
		</p>
		<p>
			<span class="count">Total Tests:</span>
			<xsl:value-of select="@total" />
		</p>
		<p>
			<span class="count">Passed:</span>
			<xsl:value-of select="@passed" />
		</p>
		<p>
			<span class="count">Failed:</span>
			<xsl:value-of select="@failed" />
		</p>
		<p>
			<span class="count">Warnings:</span>
			<xsl:value-of select="@warnings" />
		</p>
		<p>
			<span class="count">Inconclusive:</span>
			<xsl:value-of select="@inconclusive" />
		</p>
		<p>
			<span class="count">Skipped:</span>
			<xsl:value-of select="@skipped" />
		</p>
		<table>
			<thead>
				<tr>
					<th>Test Name</th>
					<th>Status</th>
				</tr>
			</thead>
			<tbody>
				<xsl:apply-templates select="//test-case" />
			</tbody>
		</table>
	</xsl:template>

	<!-- Formatting for test results -->
	<xsl:template match="test-case">
		<xsl:variable name="status" select="@result"/>
		<tr>
			<td>
				<xsl:attribute name="style">
					<xsl:choose>
						<xsl:when test="$status='Passed'">background-color: #C5E1A5;</xsl:when>
						<xsl:when test="$status='Failed'">background-color: #FFCDD2;</xsl:when>
						<xsl:when test="$status='Warning'">background-color: #FFF9C4;</xsl:when>
						<xsl:when test="$status='Inconclusive'">background-color: #E0E0E0;</xsl:when>
						<xsl:when test="$status='Skipped'">background-color: #BDBDBD;</xsl:when>
						<xsl:otherwise>background-color: #FFFFFF;</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
				<xsl:value-of select="@name"/>
			</td>
			<td>
				<xsl:value-of select="@result"/>
				<xsl:if test="$status='Failed'">
					<br />
					<xsl:value-of select="failure/message"/>
					<br />
					<xsl:value-of select="failure/stack-trace"/>
				</xsl:if>
			</td>
			<td>
				<xsl:value-of select="@time"/>
			</td>
		</tr>
	</xsl:template>

	<!-- Overall summary of test results -->
	<xsl:template match="test-run">
		<html>
			<head>
				<title>NUnit Test Results</title>
				<style>
					table, th, td {
					border: 1px solid black;
					border-collapse: collapse;
					padding: 5px;
					text-align: left;
					}
					th {
					background-color: #DDDDDD;
					}
				</style>
			</head>
			<body>
				<h1>NUnit Test Results</h1>
				<p>
					<strong>Test Run Duration:</strong>
					<xsl:value-of select="@duration"/>
					seconds<br />
					<strong>Test Result:</strong>
					<xsl:value-of select="@result"/>
					<br />
					<strong>Number of Tests:</strong>
					<xsl:value-of select="@total"/>
					<br />
					<strong>Passed:</strong>
					<xsl:value-of select="@passed"/>
					<br />
					<strong>Failed:</strong>
					<xsl:value-of select="@failed"/>
					<br />
					<strong>Warnings:</strong>
					<xsl:value-of select="@warnings"/>
					<br />
					<strong>Inconclusive:</strong>
					<xsl:value-of select="@inconclusive"/>
					<br />
					<strong>Skipped:</strong>
					<xsl:value-of select="@skipped"/>
				</p>
				<table>
					<tr>
						<th>Test Name</th>
						<th>Status</th>
						<th>Duration (s)</th>
					</tr>
					<xsl:apply-templates select="test-suite/test-case"/>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>