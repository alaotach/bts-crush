/**
 * ClubSystem — manages ARMY Clubs/guilds for collective goals and cooperative gameplay.
 *
 * Features:
 * - Create and join clubs
 * - Club-level progression and rewards
 * - Collective goals that require club-wide participation
 * - Cooperative bonuses and benefits
 * - Club events and challenges
 */

import { SaveManager } from '../storage/SaveManager';

export type ClubRole = 'member' | 'officer' | 'leader';
export type ClubGoalType =
  | 'total_matches'
  | 'total_score'
  | 'levels_completed'
  | 'specials_created'
  | 'member_xp_earned'
  | 'daily_logins'
  | 'club_challenges_won';

export interface ClubGoal {
  id: string;
  type: ClubGoalType;
  target: number;
  progress: number;
  reward: {
    coins?: number;
    xp?: number;
    exclusiveItem?: string;
  };
  description: string;
  isActive: boolean;
  expiresAt?: number; // Timestamp for time-limited goals
}

export interface ClubMember {
  playerId: string;
  username: string;
  role: ClubRole;
  joinedAt: number; // Timestamp
  contribution: {
    matches: number;
    score: number;
    levelsCompleted: number;
    specialsCreated: number;
    xpEarned: number;
    logins: number;
  };
  lastActive: number; // Timestamp
}

export interface Club {
  id: string;
  name: string;
  description: string;
  leaderId: string;
  members: ClubMember[];
  goals: ClubGoal[];
  level: number;
  xp: number;
  createdAt: number; // Timestamp
  maxMembers: number;
  isPrivate: boolean;
  inviteCode?: string;
  stats: {
    totalMatches: number;
    totalScore: number;
    totalLevelsCompleted: number;
    totalSpecialsCreated: number;
    totalXpEarned: number;
    totalLogins: number;
    weeklyActiveMembers: number;
  };
}

export class ClubSystem {
  private static instance: ClubSystem;
  private playerClub: Club | null = null;
  private saveManager: SaveManager;

  private constructor() {
    this.saveManager = SaveManager.getInstance();
    this.loadPlayerClub();
  }

  static getInstance(): ClubSystem {
    if (!ClubSystem.instance) {
      ClubSystem.instance = new ClubSystem();
    }
    return ClubSystem.instance;
  }

  private loadPlayerClub(): void {
    // In a real implementation, this would load from server
    // For this prototype, we'll check if there's club data in save data
    const saveData = this.saveManager.getData();
    // Since we don't currently store club data in save data, we'll start with no club
    // In a real game, this would fetch from server
    this.playerClub = null;
  }

  private savePlayerClub(): void {
    // In a real implementation, this would save to server
    // For local development, we could save to localStorage or AsyncStorage
    // But club data should really be server-based for true multiplayer functionality
    // For this prototype, we'll just note that we would save here
    console.log('Would save club data to server:', this.playerClub);
  }

  /**
   * Create a new club
   */
  async createClub(
    name: string,
    description: string,
    isPrivate: boolean = false,
    maxMembers: number = 30
  ): Promise<string> {
    const playerId = this.getPlayerId();
    if (!playerId) {
      throw new Error('Player ID not available');
    }

    const clubId = this.generateClubId();
    const club: Club = {
      id: clubId,
      name,
      description,
      leaderId: playerId,
      members: [{
        playerId,
        username: this.getPlayerName(),
        role: 'leader',
        joinedAt: Date.now(),
        contribution: {
          matches: 0,
          score: 0,
          levelsCompleted: 0,
          specialsCreated: 0,
          xpEarned: 0,
        },
        lastActive: Date.now(),
      }],
      goals: this.generateInitialGoals(),
      level: 1,
      xp: 0,
      createdAt: Date.now(),
      maxMembers,
      isPrivate,
      stats: {
        totalMatches: 0,
        totalScore: 0,
        totalLevelsCompleted: 0,
        totalSpecialsCreated: 0,
        totalXpEarned: 0,
        totalLogins: 0,
        weeklyActiveMembers: 1,
      },
    };

    this.playerClub = club;
    this.updatePlayerClubData(clubId);
    this.savePlayerClub();

    return clubId;
  }

  /**
   * Join an existing club
   */
  async joinClub(clubId: string): Promise<boolean> {
    // In a real implementation, this would fetch the club from a server using clubId
    // For this prototype, we'll simulate by creating a club to join
    // NOTE: This is a prototype simplification - in reality you'd join an existing club

    const playerId = this.getPlayerId();
    if (!playerId) {
      throw new Error('Player ID not available');
    }

    // Check if player is already in a club
    if (this.playerClub) {
      throw new Error('Player is already in a club');
    }

    // Create a club to join (prototype simplification)
    const club: Club = {
      id: clubId,
      name: `Club ${clubId.substring(0, 8)}`,
      description: 'A club to join',
      leaderId: 'some_other_player_id', // Not the current player
      members: [{ // Add some existing members
        playerId: 'existing_member_1',
        username: 'ExistingMember1',
        role: 'member',
        joinedAt: Date.now() - 1000 * 60 * 60 * 24, // Joined a day ago
        contribution: {
          matches: 100,
          score: 5000,
          levelsCompleted: 10,
          specialsCreated: 20,
          xpEarned: 500,
        },
        lastActive: Date.now() - 1000 * 60 * 60, // Active an hour ago
      }],
      goals: this.generateInitialGoals(),
      level: 2,
      xp: 1500,
      createdAt: Date.now() - 1000 * 60 * 60 * 24 * 3, // Created 3 days ago
      maxMembers: 30,
      isPrivate: false,
      stats: {
        totalMatches: 100,
        totalScore: 5000,
        totalLevelsCompleted: 10,
        totalSpecialsCreated: 20,
        totalXpEarned: 500,
        totalLogins: 50, // Approximate logins to match other stats
        weeklyActiveMembers: 2,
      },
    };

    // Add player to club
    const newMember: ClubMember = {
      playerId,
      username: this.getPlayerName(),
      role: 'member',
      joinedAt: Date.now(),
      contribution: {
        matches: 0,
        score: 0,
        levelsCompleted: 0,
        specialsCreated: 0,
        xpEarned: 0,
      },
      lastActive: Date.now(),
    };

    club.members.push(newMember);
    this.playerClub = club;
    this.updatePlayerClubData(clubId);
    this.savePlayerClub();

    return true;
  }

  /**
   * Leave the current club
   */
  async leaveClub(): Promise<boolean> {
    if (!this.playerClub) {
      throw new Error('Player is not in a club');
    }

    const playerId = this.getPlayerId();
    if (!playerId) {
      throw new Error('Player ID not available');
    }

    // Remove player from club
    if (this.playerClub) {
      this.playerClub.members = this.playerClub.members.filter(member => member.playerId !== playerId);

      // If the player was the leader, transfer leadership or disband club
      if (this.playerClub.leaderId === playerId) {
        if (this.playerClub.members.length > 0) {
          // Transfer to the officer with longest tenure, or first member
          const newLeader = this.playerClub.members.reduce((prev, current) =>
            (prev.joinedAt < current.joinedAt) ? prev : current
          );
          this.playerClub.leaderId = newLeader.playerId;
          // Update role in members list
          this.playerClub.members.forEach(member => {
            if (member.playerId === newLeader.playerId) {
              member.role = 'leader';
            }
          });
        } else {
          // Disband club if no members left
          this.playerClub = null;
          this.updatePlayerClubData(null);
          this.savePlayerClub();
          return true;
        }
      }

      this.updatePlayerClubData(null);
      this.savePlayerClub();
    }

    return true;
  }

  /**
   * Update player's contribution to club goals
   */
  updateContribution(
    type: keyof ClubMember['contribution'],
    amount: number
  ): void {
    if (!this.playerClub) return;

    const playerId = this.getPlayerId();
    if (!playerId) return;

    const member = this.playerClub.members.find(m => m.playerId === playerId);
    if (!member) return;

    // Update member contribution
    (member.contribution as any)[type] += amount;

    // Update club stats
    switch (type) {
      case 'matches':
        this.playerClub.stats.totalMatches += amount;
        break;
      case 'score':
        this.playerClub.stats.totalScore += amount;
        break;
      case 'levelsCompleted':
        this.playerClub.stats.totalLevelsCompleted += amount;
        break;
      case 'specialsCreated':
        this.playerClub.stats.totalSpecialsCreated += amount;
        break;
      case 'xpEarned':
        this.playerClub.stats.totalXpEarned += amount;
        break;
      case 'logins':
        this.playerClub.stats.totalLogins += amount;
        break;
    }

    // Update goal progress
    this.updateGoalProgress(this.playerClub);

    // Check if any goals are completed
    this.checkAndCompleteGoals(this.playerClub);

    this.savePlayerClub();
  }

  /**
   * Update progress for all active goals in a club
   */
  private updateGoalProgress(club: Club): void {
    club.goals.forEach(goal => {
      if (!goal.isActive) return;

      let progressAmount = 0;
      switch (goal.type) {
        case 'total_matches':
          progressAmount = club.stats.totalMatches;
          break;
        case 'total_score':
          progressAmount = club.stats.totalScore;
          break;
        case 'levels_completed':
          progressAmount = club.stats.totalLevelsCompleted;
          break;
        case 'specials_created':
          progressAmount = club.stats.totalSpecialsCreated;
          break;
        case 'member_xp_earned':
          progressAmount = club.stats.totalXpEarned;
          break;
        case 'daily_logins':
          progressAmount = club.stats.totalLogins;
          break;
        // For club_challenges_won, we'd need different tracking
        default:
          return;
      }

      goal.progress = Math.min(progressAmount, goal.target);
    });
  }

  /**
   * Check if any goals are completed and award rewards
   */
  private checkAndCompleteGoals(club: Club): void {
    club.goals.forEach(goal => {
      if (!goal.isActive) return;
      if (goal.progress >= goal.target) {
        this.completeGoal(club, goal);
      }
    });
  }

  /**
   * Complete a goal and award rewards to all members
   */
  private completeGoal(club: Club, goal: ClubGoal): void {
    goal.isActive = false;

    // Award XP to club
    const xpReward = goal.reward.xp || 500;
    club.xp += xpReward;

    // Check for club level up
    const oldLevel = club.level;
    club.level = Math.floor(club.xp / 1000) + 1; // Simple leveling formula

    if (club.level > oldLevel) {
      // Club leveled up - could unlock new features, increase max members, etc.
      club.maxMembers = Math.min(50, 20 + (club.level * 2)); // Increase max members with level
    }

    // Award rewards to all members
    club.members.forEach(member => {
      // In a real implementation, we'd add these rewards to player's inventory/currency
      // For now, we'll just log it
      console.log(`Awarding ${goal.reward.coins || 0} coins and ${goal.reward.xp || 0} XP to ${member.username} for completing club goal: ${goal.description}`);
    });

    // Generate new goal to replace completed one
    this.generateNewGoal(club);
  }

  /**
   * Generate initial goals for a new club
   */
  private generateInitialGoals(): ClubGoal[] {
    return [
      {
        id: this.generateGoalId(),
        type: 'total_matches',
        target: 1000,
        progress: 0,
        reward: { coins: 100, xp: 200 },
        description: 'Collectively make 1,000 matches',
        isActive: true,
      },
      {
        id: this.generateGoalId(),
        type: 'total_score',
        target: 50000,
        progress: 0,
        reward: { coins: 200, xp: 300 },
        description: 'Reach 50,000 total score as a club',
        isActive: true,
      },
      {
        id: this.generateGoalId(),
        type: 'levels_completed',
        target: 100,
        progress: 0,
        reward: { exclusiveItem: 'club_banner_tier1' },
        description: 'Complete 100 levels together',
        isActive: true,
      }
    ];
  }

  /**
   * Generate a new goal to replace a completed one
   */
  private generateNewGoal(club: Club): void {
    const goalTypes: ClubGoalType[] = [
      'total_matches',
      'total_score',
      'levels_completed',
      'specials_created',
      'member_xp_earned'
    ];

    const randomType = goalTypes[Math.floor(Math.random() * goalTypes.length)];
    let target = 0;
    let rewardCoins = 0;
    let rewardXp = 0;

    // Scale targets and rewards based on club level
    const levelMultiplier = 1 + (club.level * 0.5);

    switch (randomType) {
      case 'total_matches':
        target = Math.floor(500 * levelMultiplier);
        rewardCoins = Math.floor(50 * levelMultiplier);
        rewardXp = Math.floor(100 * levelMultiplier);
        break;
      case 'total_score':
        target = Math.floor(25000 * levelMultiplier);
        rewardCoins = Math.floor(100 * levelMultiplier);
        rewardXp = Math.floor(200 * levelMultiplier);
        break;
      case 'levels_completed':
        target = Math.floor(50 * levelMultiplier);
        rewardCoins = 0;
        rewardXp = 0;
        break;
      case 'specials_created':
        target = Math.floor(100 * levelMultiplier);
        rewardCoins = Math.floor(75 * levelMultiplier);
        rewardXp = Math.floor(150 * levelMultiplier);
        break;
      case 'member_xp_earned':
        target = Math.floor(10000 * levelMultiplier);
        rewardCoins = Math.floor(150 * levelMultiplier);
        rewardXp = Math.floor(250 * levelMultiplier);
        break;
    }

    const newGoal: ClubGoal = {
      id: this.generateGoalId(),
      type: randomType,
      target,
      progress: 0,
      reward: {
        coins: rewardCoins,
        xp: rewardXp,
      },
      description: this.getGoalDescription(randomType, target),
      isActive: true,
    };

    club.goals.push(newGoal);
  }

  /**
   * Get description for a goal type
   */
  private getGoalDescription(type: ClubGoalType, target: number): string {
    switch (type) {
      case 'total_matches':
        return `Collectively make ${target.toLocaleString()} matches`;
      case 'total_score':
        return `Reach ${target.toLocaleString()} total score as a club`;
      case 'levels_completed':
        return `Complete ${target} levels together`;
      case 'specials_created':
        return `Create ${target.toLocaleString()} special candies collectively`;
      case 'member_xp_earned':
        return `Earn ${target.toLocaleString()} XP as a club`;
      default:
        return `Achieve ${target} ${type}`;
    }
  }

  /**
   * Get the club the player is currently in
   */
  getPlayerClub(): Club | null {
    return this.playerClub;
  }

  /**
   * Get player's member data in their club
   */
  getPlayerClubMember(): ClubMember | null {
    const club = this.getPlayerClub();
    if (!club) return null;
    return club.members.find(m => m.playerId === this.getPlayerId()) || null;
  }

  /**
   * Update player's saved data to reflect their club membership
   */
  private updatePlayerClubData(clubId: string | null): void {
    // In a real implementation, we would update server data about club membership
    // For this prototype, we'll just note that we would update the server
    console.log(`Would update player club membership to: ${clubId}`);
  }

  /**
   * Get player ID (in a real game, this would come from auth system)
   */
  private getPlayerId(): string | null {
    // In a real implementation, this would come from authentication
    // For now, we'll use a simple placeholder or try to get from save data
    const saveData = this.saveManager.getData();
    // Since we don't currently store player ID, we'll generate a placeholder
    // In a real game, this would be tied to authentication system
    return 'player_' + Math.random().toString(36).substr(2, 9);
  }

  /**
   * Get player name (in a real game, this would come from profile/auth system)
   */
  private getPlayerName(): string {
    // Placeholder - in real game this would come from profile
    return 'ARMY_' + Math.random().toString(36).substr(2, 6).toUpperCase();
  }

  /**
   * Generate unique club ID
   */
  private generateClubId(): string {
    return 'club_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
  }

  /**
   * Generate unique goal ID
   */
  private generateGoalId(): string {
    return 'goal_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
  }

  /**
   * Start a daily login goal for the club
   */
  startDailyLoginGoal(): void {
    if (!this.playerClub) return;

    // Check if we already have an active daily login goal
    const hasDailyGoal = this.playerClub.goals.some(goal =>
      goal.type === 'daily_logins' && goal.isActive
    );

    if (!hasDailyGoal) {
      const dailyGoal: ClubGoal = {
        id: this.generateGoalId(),
        type: 'daily_logins',
        target: this.playerClub.members.length * 7, // 7 days worth of logins for all members
        progress: 0,
        reward: { coins: 300, xp: 500 },
        description: `Have all members log in for 7 consecutive days`,
        isActive: true,
        expiresAt: Date.now() + (7 * 24 * 60 * 60 * 1000), // 7 days
      };

      this.playerClub.goals.push(dailyGoal);
      this.savePlayerClub();
    }
  }

  /**
   * Handle player login - update login streak and club contributions
   * @returns login bonus coins if available
   */
  async handlePlayerLogin(saveManager: SaveManager): Promise<number> {
    // Update login streak and get potential bonus
    const bonusCoins = await saveManager.updateLoginStreak();

    // If this is a new day (streak was updated), contribute to club login count
    const hasLoggedInToday = await saveManager.hasLoggedInToday();
    if (hasLoggedInToday && this.playerClub) {
      // Increment login progress for club
      // Each member logging in counts as 1 toward login-based goals
      this.updateContribution('logins', 1);

      // Check and complete goals if needed
      this.checkAndCompleteGoals(this.playerClub);
      this.savePlayerClub();
    }

    return bonusCoins;
  }
}