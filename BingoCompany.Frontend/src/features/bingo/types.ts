export type EventSummary={id:string;name:string;publicCode:string;status:string;markingMode:string};
export type EventDetails={id:string;name:string;publicCode:string;status:string;participants:number;cards:number;rounds:{id:string;name:string;status:string}[]};
export type PrizeStage={prizeName:string;pattern:string;isActive:boolean;isCompleted:boolean};
export type PublicEvent={id:string;name:string;publicCode:string;status:string;markingMode:string;participants:number;cards:number;round?:{id:string;name:string;sequence:number;status:string;sequenceHash?:string;currentPrize?:string;stages:PrizeStage[];drawnNumbers:number[];winner?:{participantName:string;prizeName:string;pattern:string}}};
export type CardState={id:string;publicCode:string;numbers:number[][];markingMode:string;roundId?:string;roundStatus?:string;currentPrize?:string;drawnNumbers:number[];markedNumbers:number[];lastSequence:number;canGenerateNextCard:boolean};
export type PrizeDraft={sequence:number;prizeName:string;pattern:string};
