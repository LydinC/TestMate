<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs">

	<!-- Parameters -->
	<xsl:param name="pass-color" select="'#28a745'"/>
	<xsl:param name="fail-color" select="'#dc3545'"/>
	<xsl:param name="skip-color" select="'#ffc107'"/>

	<!-- Key for grouping tests by status -->
	<xsl:key name="test-by-status" match="test-case" use="@result"/>

	<!-- Entry point for transformation -->
	<xsl:template match="/test-run">
		<html>
			<head>
				<title>NUnit Test Report</title>
				<style>
					/* Test card styles */
					.test-card {
					border: 1px solid #dee2e6;
					border-radius: 5px;
					margin: 10px;
					padding: 10px;
					}
					.test-card h4 {
					margin: 0;
					}
					.test-card.passed {
					border-color: #28a745;
					}
					.test-card.failed {
					border-color: #dc3545;
					}
					.test-card.skipped {
					border-color: #ffc107;
					}

					/* Pie chart styles */
					.chart-container {
					width: 500px;
					margin: 20px auto;
					text-align: center;
					}
					.chart-legend {
					display: inline-block;
					width: 10px;
					height: 10px;
					margin-right: 10px;
					}
				</style>
			</head>
			<body>
				<!-- Test run summary -->
				<div class="chart-container">
					<svg width="100%" height="100%">
						<g>
							<xsl:variable name="total" select="count(test-case)"/>
							<xsl:variable name="passed" select="count(test-case[@result='Passed'])"/>
							<xsl:variable name="failed" select="count(test-case[@result='Failed'])"/>
							<xsl:variable name="skipped" select="count(test-case[@result='Skipped'])"/>
							<xsl:variable name="passed-percent" select="format-number($passed div $total * 100, '0.#')"/>
							<xsl:variable name="failed-percent" select="format-number($failed div $total * 100, '0.#')"/>
							<xsl:variable name="skipped-percent" select="format-number($skipped div $total * 100, '0.#')"/>

							<circle cx="50%" cy="50%" r="100" fill="#ddd"/>
							<circle cx="50%" cy="50%" r="100" stroke="#fff" stroke-width="50"/>

							<xsl:if test="$passed > 0">
								<circle cx="50%" cy="50%" r="100" fill="{$pass-color}" stroke="none">
									<xsl:attribute name="stroke-dasharray">
										<xsl:value-of select="$passed-percent"/>,<xsl:value-of select="100 - $passed-percent"/>
									</xsl:attribute>
								</circle>
							</xsl:if>

							<xsl:if test="$failed > 0">
								<circle cx="50%" cy="50%" r="100" fill="{$fail-color}" stroke="none">
									<xsl:attribute name="stroke-dasharray">
										<xsl:value-of select="$failed-percent"/>,<xsl:value-of select="100 - $failed-percent"/>
									</xsl:attribute>
								</circle>
							</xsl:if>

							<xsl:if test="$skipped > 0">
								<circle cx="50%" cy="50%" r="100" fill="{$skip-color}" stroke="none">
									<xsl:attribute name="stroke-dasharray">
										<xsl:value-of select="$skipped-percent"/>,<xsl:value-of select="100 - $skipped-percent"/>
									</xsl:attribute>
								</circle>
							</xsl:if>

							<text x="50%" y="50%" text-anchor="middle" dominant-baseline="central" font-size="48">
								<xsl:value-of select="$total"/>
							</text>
							<text x="50%" y="65%" text-anchor="middle" dominant-baseline="central" font-size="24">
								Total
							</text>

							<text x="20%" y="85%" font-size="24" fill="{$pass-color}">
								<xsl:value-of select="$passed"/>
								<tspan dx="5">/</tspan>
								<xsl:value-of select="$passed-percent"/>
								<tspan dx="5">%</tspan>
								<tspan dx="10" class="chart-legend" style="background-color:{$pass-color}"></tspan>
								Passed
							</text>

							<text x="50%" y="85%" font-size="24" fill="{$fail-color}">
								<xsl:value-of select="$failed"/>
								<tspan dx="5">/</tspan>
								<xsl:value-of select="$failed-percent"/>
								<tspan dx="5">%</tspan>
								<tspan dx="10" class="chart-legend" style="background-color:{$fail-color}"></tspan>
								Failed
							</text>

							<text x="80%" y="85%" font-size="24" fill="{$skip-color}">
								<xsl:value-of select="$skipped"/>
								<tspan dx="5">/</tspan>
								<xsl:value-of select="$skipped-percent"/>
								<tspan dx="5">%</tspan>
								<tspan dx="10" class="chart-legend" style="background-color:{$skip-color}"></tspan>
								Skipped
							</text>
						</g>
					</svg>
				</div>

				<!-- Test cards grouped by status -->
				<xsl:for-each select="test-case[generate-id() = generate-id(key('test-by-status', @result)[1])]">
					<h3>
						<xsl:value-of select="@result"/>
					</h3>
					<xsl:variable name="status" select="@result"/>
					<xsl:for-each select="key('test-by-status', $status)">
						<div class="test-card {$status}">
							<h4>
								<xsl:value-of select="@name"/>
							</h4>
							<p>
								Status: <strong>
									<xsl:value-of select="@result"/>
								</strong>
								<br/>
								Duration: <xsl:value-of select="format-number(@duration, '0.000')"/> seconds
							</p>
							<xsl:if test="@result = 'Failed'">
							<div class="test-failure">
								<p>
								Failure message: <xsl:value-of select="failure/message"/>
								</p>
								<p>
								Stack trace:
								<xsl:call-template name="indent">
									<xsl:with-param name="text" select="failure/stack-trace"/>
								</xsl:call-template>
								</p>
							</div>
							</xsl:if>
							<xsl:if test="@result = 'Passed'">
							<div class="test-success">
								<p>
								Success message: <xsl:value-of select="output"/>
								</p>
							</div>
							</xsl:if>
						</div>
						</xsl:for-each>
					</xsl:for-each>
					
				</body>
		</html>
	</xsl:template>
	<!-- Indent template for stack trace -->
	<xsl:template name="indent">
		<xsl:param name="text"/>
		<xsl:param name="indent" select="' '"/>
		<xsl:choose>
			<xsl:when test="contains($text, '')">
				<xsl:value-of select="substring-before($text, '')"/>
				<br/>
				<xsl:call-template name="indent">
					<xsl:with-param name="text" select="substring-after($text, '')"/>
					<xsl:with-param name="indent" select="$indent"/>
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>