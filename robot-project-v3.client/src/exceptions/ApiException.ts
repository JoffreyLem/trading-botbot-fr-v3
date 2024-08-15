export class ApiException extends Error {
  constructor(message: string = "Une erreur est survenu.") {
    super(message);
    this.name = "ApiException";
  }
}
