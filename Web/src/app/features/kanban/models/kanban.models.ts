export enum CardStatus {
  Plan = 0,
  ToDo = 1,
  Doing = 2,
  Done = 3
}

export enum CardScope {
  Personal = 0,
  Organization = 1
}

export interface CardTask {
  id: string;
  cardId: string;
  title: string;
  isCompleted: boolean;
  assigneeId: number | null;
  assigneeName: string | null;
  sequenceOrder: number;
  dueDate: string | null;
  devOpsUrl: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface KanbanCard {
  id: string;
  title: string;
  description: string | null;
  status: CardStatus;
  scope: CardScope;
  ownerId: number;
  ownerName: string | null;
  departmentId: number | null;
  departmentName: string | null;
  dueDate: string | null;
  sequenceOrder: number;
  devOpsUrl: string | null;
  createdAt: string;
  updatedAt: string;
  tasks: CardTask[];
}

export interface MoveCardRequest {
  status: CardStatus;
  sequenceOrder: number;
  updatedAt: string;
}
