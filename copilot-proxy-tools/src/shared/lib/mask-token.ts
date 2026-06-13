export function maskToken(token) {
  if (!token) {
    return '';
  }
  return `${token.slice(0, 10)}...${token.slice(-5)}`;
}
