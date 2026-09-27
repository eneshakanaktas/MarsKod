// Yazı tipleri (Google Fonts, SIL Open Font License: ücretsiz, ticari kullanıma açık, Türkçe harfler var).
// Başlıklar: Chakra Petch (bilim kurgu havası). Kod: JetBrains Mono (okunaklı, karakterler karışmaz).

import { ChakraPetch_500Medium, ChakraPetch_600SemiBold, ChakraPetch_700Bold } from '@expo-google-fonts/chakra-petch';
import { JetBrainsMono_400Regular, JetBrainsMono_600SemiBold } from '@expo-google-fonts/jetbrains-mono';

export const FONT_FILES = {
  ChakraPetch_500Medium,
  ChakraPetch_600SemiBold,
  ChakraPetch_700Bold,
  JetBrainsMono_400Regular,
  JetBrainsMono_600SemiBold,
};

export const fonts = {
  body: 'ChakraPetch_500Medium',
  heading: 'ChakraPetch_700Bold',
  label: 'ChakraPetch_600SemiBold',
  code: 'JetBrainsMono_400Regular',
  codeBold: 'JetBrainsMono_600SemiBold',
} as const;
