Feature: Search

Scenario: Search for active music contracts
	Given the supplied reference data
	When user perform search by ITunes 03-01-2012
	Then the output should be
	| Artist       | Title                   | Usages           | StartDate    | EndDate |
	| Monkey Claw  | Black Mountain          | digital download | 02-01-2012   |         |
	| Monkey Claw  | Motor Mouth             | digital download | 03-01-2011   |         |
	| Tinie Tempah | Frisky (Live from SoHo) | digital download | 02-01-2012   |         |
	| Tinie Tempah | Miami 2 Ibiza           | digital download | 02-01-2012   |         |

Scenario: Search for active music contracts_2
	Given the supplied reference data
	When user perform search by YouTube 12-27-2012
	Then the output should be
	| Artist       | Title                   | Usages    | StartDate     | EndDate       |
	| Monkey Claw  | Christmas Special       | streaming | 12-25-2012    | 12-31-2012    |
	| Monkey Claw  | Iron Horse              | streaming | 06-01-2012    |               |
	| Monkey Claw  | Motor Mouth             | streaming | 03-01-2011    |               |
	| Tinie Tempah | Frisky (Live from SoHo) | streaming | 02-01-2012    |               |

Scenario: Search for active music contracts_3
	Given the supplied reference data
	When user perform search by YouTube 04-01-2012
	Then the output should be
	| Artist       | Title                   | Usages    | StartDate    | EndDate |
	| Monkey Claw  | Motor Mouth             | streaming | 03-01-2011   |         |
	| Tinie Tempah | Frisky (Live from SoHo) | streaming | 02-01-2012   |         |

