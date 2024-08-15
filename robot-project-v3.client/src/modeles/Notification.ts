export interface Notification {
  id: string;
  message: string;
  info?: string;
  type: "error" | "info";
}
