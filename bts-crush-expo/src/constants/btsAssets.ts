export type BTSMember = 'rm' | 'jin' | 'suga' | 'jhope' | 'jimin' | 'v' | 'jungkook';

export interface BTSMemberAsset {
  id: BTSMember;
  displayName: string;
  themeColor: string;
  image: any;
}

export const BTS_MEMBERS: Record<BTSMember, BTSMemberAsset> = {
  rm: {
    id: 'rm',
    displayName: 'RM',
    themeColor: '#3498db',
    image: require('../../assets/members/rm.png'),
  },
  jin: {
    id: 'jin',
    displayName: 'Jin',
    themeColor: '#ff9ff3',
    image: require('../../assets/members/jin.png'),
  },
  suga: {
    id: 'suga',
    displayName: 'SUGA',
    themeColor: '#9b59b6',
    image: require('../../assets/members/suga.png'),
  },
  jhope: {
    id: 'jhope',
    displayName: 'j-hope',
    themeColor: '#f1c40f',
    image: require('../../assets/members/jhope.png'),
  },
  jimin: {
    id: 'jimin',
    displayName: 'Jimin',
    themeColor: '#ffb8b8',
    image: require('../../assets/members/jimin.png'),
  },
  v: {
    id: 'v',
    displayName: 'V',
    themeColor: '#1abc9c',
    image: require('../../assets/members/v.png'),
  },
  jungkook: {
    id: 'jungkook',
    displayName: 'Jung Kook',
    themeColor: '#2980b9',
    image: require('../../assets/members/jungkook.png'),
  }
};

export const ALL_MEMBERS = Object.values(BTS_MEMBERS);
