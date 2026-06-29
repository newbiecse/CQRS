import { OpenAIChatProvider, XRequest } from '@ant-design/x-sdk';
import { getAccessToken } from '@/utils/auth';

export const CHAT_API_URL = '/api/chat/completions';

export const createChatProvider = () =>
  new OpenAIChatProvider({
    request: XRequest(CHAT_API_URL, {
      manual: true,
      params: { model: 'shop-agent', stream: true },
      headers: () => {
        const token = getAccessToken();
        return token ? { Authorization: `Bearer ${token}` } : {};
      },
    }),
  });
