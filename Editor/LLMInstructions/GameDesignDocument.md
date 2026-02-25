# Game Design Document

## 1. Game Overview

**Title**: Strategy Game Template (Working Title)

**Genre**: Turn-Based Strategy, 4X Lite

**Inspiration**: Total War series - focusing on province management, resource gathering, and army building without real-time tactical combat

**Core Concept**: Players assume the role of a faction leader managing provinces (map tiles), recruiting and commanding armies, constructing buildings, and managing resources across a medieval world. The game emphasizes strategic planning, resource management, and territorial conquest through turn-based gameplay.

**Target Audience**: Strategy game enthusiasts aged 16+, particularly fans of turn-based grand strategy games

---

## 2. Design Pillars

1. **Strategic Depth** - Every decision matters. Province management, resource allocation, and army composition require careful planning and long-term thinking.

2. **Accessibility** - Turn-based gameplay allows players to think strategically without time pressure. Clean UI provides clear information for informed decision-making.

3. **Replayability** - Procedural and random events, multiple factions with distinct playstyles, and emergent gameplay ensure no two playthroughs are identical.

4. **Player Agency** - Players control their empire's growth trajectory through economic decisions, military strategy, diplomatic choices (future), and territorial expansion.

5. **Meaningful Progression** - Early-game survival, mid-game expansion, and late-game domination create clear progression loops with escalating challenges.

---

## 3. Game World

### Setting
Medieval fantasy world divided into provinces (map tiles). Each province has unique characteristics, resources, and strategic value. Factions vie for dominance through expansion and conquest.

### Map System
- **Grid-Based Provinces**: The world is divided into hexagonal or square grid tiles, each representing a province/territory
- **Faction Territory**: Each province belongs to a faction (Red, Blue, Green, Yellow) or is neutral
- **Visual Representation**: Tiles display faction colors, population density, military presence, and resource icons
- **Strategic Locations**: Some tiles offer higher resource yields or defensive bonuses

### Factions
Five playable factions with distinct visual themes:
- **Red Faction** - Aggressive military focus
- **Blue Faction** - Balanced approach
- **Green Faction** - Resource-focused
- **Yellow Faction** - Economic powerhouse
- **Neutral Territory** - Can be conquered and claimed

---

## 4. Core Gameplay Systems

### 4.1 Province Management System

**Overview**: Provinces (map tiles) are the foundation of your empire. Each province generates resources, can be improved through construction, and houses a population.

**Province Attributes**:
- **Population**: Base resource pool for recruitment and taxation
- **Happiness**: Affects tax revenue and stability (morale indicator)
- **Population Growth**: Each turn, population increases based on happiness and available resources
- **Ownership**: Which faction controls the province
- **Garrison**: Military units stationed for defense

**Province Mechanics**:
- **Taxation**: Players extract gold from a province based on population and happiness
- **Resource Generation**: Each province generates primary resources (Food, Wood, Stone, Iron) based on tile type and constructed buildings
- **Population Happiness**: Influenced by:
  - Tax rate (higher taxes reduce happiness)
  - Available food (insufficient food reduces happiness)
  - Presence of public buildings (markets, temples - future implementation)
  - Recent conquest or conflict (temporary unhappiness)

**Province Development**:
- Provinces can be improved through building construction
- Higher development leads to increased resource generation and population capacity

### 4.2 Resource System

**Primary Resources**: Gold, Food, Wood, Stone, Iron

**Resource Generation**:
- **Gold**: Generated through taxation of provinces
- **Food**: Produced by agricultural buildings and fertile tile types
- **Wood**: Generated from forests and lumber operations
- **Stone**: Quarried from mountainous regions
- **Iron**: Extracted from ore deposits

**Resource Usage**:
- **Unit Recruitment**: Armies consume gold and resources to recruit and maintain units
- **Building Construction**: All buildings require specific resource combinations
- **Unit Upkeep**: Armies cost resources per turn to maintain (morale, equipment, provisions)

**Resource Management**:
- Players have finite resources each turn
- Strategic decisions: Invest in growth vs. military expansion
- Resource shortages impact army morale and provincial stability
- Trade (future feature) could balance regional resource disparities

### 4.3 Army System

**Overview**: Armies are military forces composed of different unit types, each with distinct roles and characteristics. Armies can be recruited in provinces with sufficient recruitment pools and garrisoned in conquered territories.

**Unit Categories**:
- **Infantry**: Balanced, reliable ground troops
- **Cavalry**: Fast, mobile units with high mobility
- **Artillery**: High damage output, effective against grouped enemies
- **Ranged**: Ranged combat specialists, good for support

**Unit Attributes** (per unit in an army):
- **Type**: Infantry, Cavalry, Artillery, or Ranged
- **Quantity**: Squad size (number of soldiers in the unit)
- **Health**: Current status of the unit
- **Morale**: Combat effectiveness and susceptibility to routing
- **Experience** (future): Units gain experience and improve over time

**Commander System**:
- Each army has a commander with unique attributes
- Commander influences army morale, movement range, and special abilities (future)
- Commanders can be recruited or captured

**Army Mechanics**:
- **Recruitment**: Players recruit units from provinces with available population
- **Movement**: Armies move across the map one province per turn (movement range may vary with future improvements)
- **Garrison**: Armies can be stationed in provinces for defense
- **Merging**: Multiple armies can be combined into larger forces
- **Splitting**: Large armies can be split into smaller forces
- **Morale**: Army combat effectiveness and morale decay over time. Low morale causes penalties or routs

**Army Maintenance**:
- Each army costs gold per turn for upkeep
- Armies without adequate supply suffer morale penalties
- Hungry armies consume food from provincial stockpiles

### 4.4 Combat System

**Overview**: Combat occurs when two armies occupy the same province or when one attacks a defended garrison. No real-time tactical battles; instead, outcomes are calculated based on unit composition, morale, and commander attributes.

**Combat Resolution**:
- **Automatic Resolution**: Combat outcomes determined by algorithm comparing:
  - Army composition and unit types
  - Unit health and morale
  - Commander bonuses
  - Defensive terrain bonuses (if applicable)
  - Garrison strength

**Combat Outcomes**:
- **Victory**: Attacker takes control of province; defender's army destroyed or routed
- **Defeat**: Attacker's army destroyed or routed; defender retains province
- **Losses**: Both armies take casualties based on battle intensity
- **Morale Impact**: Winning/losing units affects morale for future combats

**Post-Combat**:
- Winner gains control of the province
- Defeated army is destroyed; survivors may retreat (future feature)
- Winner can occupy, garrison, or continue moving
- Casualties must be replaced through recruitment

### 4.5 Building & Construction System

**Overview**: Buildings are permanent improvements to provinces that enhance resource generation, military capacity, or public welfare. Construction is turn-based and requires resources and time to complete.

**Building Types**:

**Economic Buildings**:
- **Market**: Increases gold generation from taxation
- **Granary**: Increases food production capacity
- **Lumber Mill**: Increases wood generation
- **Stone Quarry**: Increases stone generation
- **Iron Mine**: Increases iron generation

**Military Buildings**:
- **Barracks**: Increases recruitment pool for Infantry units
- **Stables**: Increases recruitment pool for Cavalry units
- **Armory**: Increases recruitment pool for Ranged units
- **Siege Workshop**: Increases recruitment pool for Artillery units
- **Watchtower**: Provides defensive bonuses to garrison

**Infrastructure Buildings**:
- **Roads**: Improve movement efficiency (future feature)
- **Bridge**: Allow crossing water barriers (future feature)

**Public Buildings**:
- **Temple**: Increases population happiness
- **Town Hall**: Increases administrative capacity
- **Hospital**: Improves population growth rate

**Construction Mechanics**:
- **Construction Queue**: Players can queue multiple buildings in a province
- **Turn-Based Progress**: Each turn, construction progresses by a fixed percentage
- **Resource Consumption**: Resources are consumed each turn during construction (or lump-sum at completion)
- **Cancellation**: Players can cancel incomplete buildings, losing partial resources
- **Multiple Construction**: Only one building can be under construction per province at a time (initially)

**Building Requirements**:
- Each building requires:
  - **Gold Cost**: Base construction cost
  - **Resource Cost**: Specific combination of resources (Wood, Stone, Iron, Food)
  - **Construction Time**: Number of turns required
  - **Prerequisites**: Some buildings may require other buildings to exist first

**Building Benefits**:
- Enhanced resource generation (economic buildings)
- Increased recruitment capacity (military buildings)
- Population growth and happiness (infrastructure buildings)
- Defensive bonuses (watchtower)

### 4.6 Turn System

**Overview**: The game progresses through turns, with each turn representing a seasonal period. The turn sequence provides structure and allows asynchronous planning.

**Turn Structure**:
1. **Player Actions Phase**:
   - Select and move armies
   - Queue construction projects
   - Garrison forces
   - Recruit units (if recruitment capacity available)

2. **Automatic Phase**:
   - Resource generation (all provinces)
   - Construction progress (all buildings)
   - Unit recruitment (auto-fill if resources available)
   - Population growth
   - Army maintenance costs applied

3. **End Turn**:
   - Random events may trigger
   - Turn advances
   - Season advances (Spring → Summer → Fall → Winter → Spring)
   - Year counter increments when Winter completes

**Seasonal System**:
- **Spring**: Standard resource generation, favorable growth conditions
- **Summer**: Increased food production, optimal for military campaigns
- **Fall**: Moderate resource generation, harvest bonuses
- **Winter**: Reduced resource generation, harder survival conditions, movement penalties (future)

**Turn Limits** (optional):
- Victory condition: Achieve goal within X turns
- Encourages strategic planning and quick execution

### 4.7 Events System

**Overview**: Random events introduce unpredictability and compelling story moments that affect gameplay.

**Event Types**:

**Crisis Events**:
- **Plague**: Reduces population and happiness in affected province
- **Famine**: Reduces food availability, population starvation
- **Rebellion**: Province's population turns against ruler, must suppress

**Opportunity Events**:
- **Trade Opportunity**: Gain bonus resources
- **Recruit Heroes**: Powerful commander available for recruitment
- **Resource Windfall**: Unexpected resource gain

**Neutral Events**:
- **Population Boom**: Exceptional population growth
- **Bandit Activity**: Minor gold loss
- **Weather Events**: Temporary effects on resource generation

**Event Triggers**:
- Random chance each turn
- Triggered by game state (low happiness, low resources, etc.)
- Triggered by player actions (conquest, heavy taxation)

**Event Resolution**:
- Player presented with event popup
- May offer choices (attack rebel army, pay tribute, negotiate)
- Consequences affect province and empire

---

## 5. UI/UX Flows

### 5.1 Main Game View (Default Map View)

**Primary Display**:
- Map showing all provinces with faction colors
- Province information overlay (population, resources, garrison)
- Current season and year display
- Resource counter (Gold, Food, Wood, Stone, Iron)
- Army information for selected army

**Interactions**:
- Click tile to select province
- Right-click or hold to view province details
- Click army to select and view options
- Keyboard/mouse controls for camera movement and zoom

### 5.2 Province Management Panel

**Displays When Province Selected**:
- Province name and ownership
- Population and happiness metrics
- Current resource generation rate
- Building queue and construction progress
- Garrison forces and available recruitment
- Tax rate adjustment slider
- Construction menu (list of buildable structures)

**Available Actions**:
- Queue construction
- Adjust tax rate
- Recruit units
- Disband garrison

### 5.3 Army View

**Displays When Army Selected**:
- Army composition (list of unit types and quantities)
- Total unit count
- Commander information and attributes
- Current morale
- Movement range and current location
- Army maintenance cost per turn

**Available Actions**:
- Move army (highlight valid destinations)
- Attack adjacent enemy army or garrison
- Merge with nearby army
- Split army (if composition allows)
- Garrison in current province
- Disband army

### 5.4 Army Recruitment View

**Displays When Recruiting**:
- Available unit types (based on province buildings)
- Unit cost breakdown (gold + resources)
- Current available resources
- Recruitment limit (based on population)
- Current recruitment queue

**Available Actions**:
- Select unit type
- Adjust quantity
- Confirm recruitment (resources deducted immediately or per turn)
- Cancel recruitment

### 5.5 Town/Province Construction View

**Displays When Province Construction Selected**:
- List of constructible buildings
- Building stats and benefits
- Resource cost breakdown
- Construction time estimate
- Prerequisites (if any)
- Current construction queue

**Available Actions**:
- Select building to construct
- Add to queue
- View building details
- Cancel construction project

### 5.6 Event Popup

**Displays When Event Triggered**:
- Event title and description
- Event type icon
- Choice options (if applicable)
- Consequences preview

**Available Actions**:
- Select response option (Accept, Reject, Negotiate, etc.)
- Confirm choice

### 5.7 End Turn Confirmation

**Displays Before Turn Advance**:
- Summary of pending actions
- Resource allocation summary
- Warning for critical issues (army starvation, recruitment overages)
- Current turn and season info

**Available Actions**:
- Confirm turn end
- Return to planning phase

---

## 6. Progression & Win Conditions

### Victory Conditions (Optional/Configurable)

1. **Territorial Domination**: Control a certain percentage of the map (e.g., 60%)
2. **Resource Accumulation**: Accumulate a target amount of gold (e.g., 100,000 gold)
3. **Military Superiority**: Maintain the largest standing army for a set number of turns
4. **Conquest Victory**: Eliminate all enemy factions (destroy all enemy armies and conquer all territory)
5. **Time-Based Victory**: Achieve the highest score after X turns

### Scoring System
- Territory controlled: Points per province owned
- Total military strength: Points per army size
- Resource wealth: Points per accumulated resources
- Buildings constructed: Points per building
- Allies/Vassals (future): Diplomatic achievements

### Difficulty Modifiers
- **Easy**: Increased resource generation, reduced enemy aggression
- **Normal**: Balanced gameplay
- **Hard**: Reduced resources, aggressive enemy AI, random events more frequent
- **Ironman**: Permadeath, no save-scumming

---

## 7. Game Feel & Pacing

### Aesthetic
- Medieval fantasy setting with clear faction color differentiation
- Readable map with intuitive province representations
- Clear visual hierarchy for important information
- Minimal particle effects, focus on clarity

### Audio (Future Implementation)
- Ambient background music varying by season
- UI sound effects for interactions
- Victory/defeat fanfares
- Notification sounds for important events

### Pacing
- **Early Game** (Turns 1-20): Territorial expansion, establishing production
- **Mid Game** (Turns 21-50): Economic and military growth, inter-faction competition
- **Late Game** (Turns 50+): Consolidated power, final territorial disputes, domination victory path

### Player Feedback
- Clear indication of what actions are valid (grayed-out buttons for unavailable actions)
- Tooltips explaining mechanics and costs
- Resource change indicators showing gains/losses
- Event notifications for significant changes

---

## 8. Gameplay Example

**Scenario**: A player starts as the Blue Faction controlling three provinces.

**Turn 1-3**:
- Claim nearby neutral provinces to establish territory
- Begin constructing economic buildings (granary, lumber mill)
- Recruit first infantry units for defense

**Turn 4-10**:
- Growing resource income supports army expansion
- Encounter Red Faction armies near border
- Recruit cavalry and ranged units for tactical diversity
- Construct barracks and stables to increase recruitment capacity

**Turn 11-20**:
- First major conflict with Red Faction over border province
- Manage casualties and recruit replacements
- Continue economic development
- Event: Plague hits one province, temporary population loss

**Turn 21+**:
- Consolidate gains, establish military dominance
- Plan attack on Yellow Faction's weak province
- Manage multiple armies across expanding territory
- Build toward victory condition (territorial control or total conquest)

---

## 9. Future Features & Expansions

### Phase 2 Features
- **Diplomacy System**: Treaties, alliances, vassalization
- **Technology Tree**: Research improvements for economic/military bonuses
- **Heroes & Champions**: Named leaders with unique abilities
- **Naval System**: Ships for movement and coastal combat
- **Espionage**: Spying on factions, sabotage, assassination

### Phase 3 Features
- **Multi-Player**: Simultaneous or turn-based multiplayer campaigns
- **Procedural Generation**: Randomized maps and resources
- **Advanced Combat**: Unit formations, flanking bonuses, terrain effects
- **Siege Mechanics**: Formal siege system for fortified cities
- **Economy Depth**: Caravans, trade routes, market fluctuations

### Post-Launch Content
- **Campaign Maps**: Handcrafted scenarios with narrative
- **Modding Support**: Allow creation of custom units, buildings, factions
- **Balance Patches**: Ongoing tuning based on player feedback
- **New Factions**: Additional playable civilizations

---

## 10. Design Philosophy & Notes

### Simplicity First
- Turn-based system removes time pressure, letting players think strategically
- Automatic combat avoids complex tactical battles, focusing on strategic army composition and positioning
- Clear UI prevents information overload

### Strategic Complexity
- Resource management creates economic strategy layer
- Army composition and positioning create tactical depth
- Event system introduces uncertainty and decision-making opportunities
- Multiple paths to victory encourage varied playstyles

### Replayability
- Random events and map variations
- Different faction characteristics
- Multiple victory conditions
- Scaling difficulty levels

### Player Autonomy
- No mandatory story; players write their own narrative
- No "best" strategy; multiple viable approaches
- Flexible building/recruitment systems allow creative solutions

---

## Appendix A: Glossary

- **Province**: A hexagonal/square tile representing a territory or town
- **Army**: A collection of military units under a commander
- **Unit**: A squad of soldiers of a specific type (Infantry, Cavalry, etc.)
- **Garrison**: Military units stationed in a province for defense
- **Recruitment**: Process of creating new units from population
- **Morale**: Army combat effectiveness and stability metric
- **Faction**: One of five playable civilizations
- **Turn**: One game cycle (season)
- **Building**: Permanent province improvement providing benefits
- **Construction**: The turn-based building process
- **Resources**: Gold, Food, Wood, Stone, Iron - used for various game actions
- **Event**: Random occurrence affecting gameplay

---

**Document Version**: 1.0
**Last Updated**: January 2026