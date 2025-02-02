#Generates a C# Reloaded delegate and hook for the selected function, with
#a pattern generated with makesig by nosoop.
#@author nosoop, RyoTune
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 

from __future__ import print_function

import collections
import ghidra.program.model.lang.OperandType as OperandType
import ghidra.program.model.lang.Register as Register
import ghidra.program.model.address.AddressSet as AddressSet

MAKE_SIG_AT = collections.OrderedDict([
  ('fn', 'start of function'),
  ('cursor', 'instruction at cursor')
])

BytePattern = collections.namedtuple('BytePattern', ['is_wildcard', 'byte'])

def __bytepattern_ida_str(self):
  # return an IDA-style binary search string
  return '{:02X}'.format(self.byte) if not self.is_wildcard else '??'

def __bytepattern_sig_str(self):
  # return a SourceMod-style byte signature
  return r'\x{:02X}'.format(self.byte) if not self.is_wildcard else r'\x2A'

BytePattern.ida_str = __bytepattern_ida_str
BytePattern.sig_str = __bytepattern_sig_str

def dumpOperandInfo(ins, op):
  t = hex(ins.getOperandType(op))
  print('  ' + str(ins.getPrototype().getOperandValueMask(op)) + ' ' + str(t))
  
  # TODO if register
  for opobj in ins.getOpObjects(op):
    print('  - ' + str(opobj))

def shouldMaskOperand(ins, opIndex):
  """
  Returns True if the given instruction operand mask should be masked in the signature.
  """
  optype = ins.getOperandType(opIndex)
  # if any(reg.getName() == "EBP" for reg in filter(lambda op: isinstance(op, Register), ins.getOpObjects(opIndex))):
    # return False
  return optype & OperandType.DYNAMIC or optype & OperandType.ADDRESS

def getMaskedInstruction(ins):
  """
  Returns a generator that outputs either a byte to match or None if the byte should be masked.
  """
  # print(ins)
  
  # resulting mask should match the instruction length
  mask = [0] * ins.length
  
  proto = ins.getPrototype()
  # iterate over operands and mask bytes
  for op in range(proto.getNumOperands()):
    # dumpOperandInfo(ins, op)
    
    # TODO deal with partial byte masks
    if shouldMaskOperand(ins, op):
      mask = [ m | v & 0xFF for m, v in zip(mask, proto.getOperandValueMask(op).getBytes()) ]
  # print('  ' + str(mask))
  
  for m, b in zip(mask, ins.getBytes()):
    if m == 0xFF:
      # we only check for fully masked bytes at the moment
      yield BytePattern(is_wildcard = True, byte = None)
    else:
      yield BytePattern(byte = b & 0xFF, is_wildcard = False)

# removes trailing wilds from the sig
def cleanupWilds(byte_pattern):
  for byte in reversed(byte_pattern):
    if byte.is_wildcard is False:
      break
    del byte_pattern[-1]
    
def getPattern(fn, cm, start_at = MAKE_SIG_AT['fn'], min_length = 1):
  if start_at == MAKE_SIG_AT['fn']:
    ins = cm.getInstructionAt(fn.getEntryPoint())
  elif start_at == MAKE_SIG_AT['cursor']:
    try:
      # Ghidra 10.4 introduces an additional parameter 'usePrototypeLength'
      # it will throw on older versions, so fall back to the previous version
      ins = cm.getInstructionContaining(currentAddress, False)
    except TypeError:
      ins = cm.getInstructionContaining(currentAddress)
  
  if not ins:
    raise Exception("Could not find entry point to function")

  pattern = "" # contains pattern string (supports regular expressions)
  byte_pattern = [] # contains BytePattern instances
  
  # keep track of our matches
  matches = []
  match_limit = 128
  
  while fm.getFunctionContaining(ins.getAddress()) == fn:
    for entry in getMaskedInstruction(ins):
      byte_pattern.append(entry)
      if entry.is_wildcard:
        pattern += '.'
      else:
        pattern += r'\x{:02x}'.format(entry.byte)
    
    expected_next = ins.getAddress().add(ins.length)
    ins = ins.getNext()
    
    if ins.getAddress() != expected_next:
      # add wildcards until we get to the next instruction
      for _ in range(ins.getAddress().subtract(expected_next)):
        byte_pattern.append(BytePattern(is_wildcard = True, byte = None))
        pattern += '.'
    
    if len(byte_pattern) < min_length:
      continue
    
    if 0 < len(matches) < match_limit:
      # we have all the remaining matches, start only searching those addresses
      match_set = AddressSet()
      for addr in matches:
        match_set.add(addr, addr.add(len(byte_pattern)))
      matches = findBytes(match_set, pattern, match_limit, 1)
    else:
      # the matches are sorted in ascending order, so the first match will be the start
      matches = findBytes(matches[0] if len(matches) else None, pattern, match_limit)
    
    if len(matches) < 2:
      break
  
  if not len(matches) == 1:
    print(*(b.ida_str() for b in byte_pattern))
    print('Signature matched', len(matches), 'locations:', *(matches))
    printerr("Could not find unique signature")
  else:
    cleanupWilds(byte_pattern)
    #print("Signature for", fn.getName())
    #print(" ".join(b.ida_str() for b in byte_pattern))
    return " ".join(b.ida_str() for b in byte_pattern)
  
def getCsharpType(in_type):
    in_type = in_type.replace(" ", "")
    if ("undefined8" == in_type
        or "longlong" in in_type
        or "size_t" in in_type):
      return (in_type
            .replace("undefined8", "nint")
            .replace("ulonglong", "nint")
            .replace("longlong", "nint")
            .replace("size_t", "nint"));

    if "undefined4" in in_type:
      return in_type.replace("undefined4", "int");

    if "undefined2" in in_type:
      return in_type.replace("undefined2", "short");

    if ("undefined" in in_type):
      return in_type.replace("undefined", "void");

    if ("char" in in_type):
      return in_type.replace("char", "byte");

    return in_type

def getCsharpArgs(in_func):
  args = {}
  for param in in_func.getParameters():
    args[param.getName()] = getCsharpType(param.getDataType().getName())
  return args

def getCsharpFunction(fn_name, fn_ret, fn_args):
  return "{} {}({})".format(fn_ret, fn_name, ", ".join("{} {}".format(value, key) for key, value in fn_args.items()))

def getReloadedDelegate(fn_name, fn_ret, fn_args):
  return "private delegate {};".format(getCsharpFunction(fn_name, fn_ret, fn_args))

def getImplName(fn_name):
  return "{}Impl".format(fn_name)

def getReloadedImpl(fn_name, fn_ret, fn_args):
  funcSig = getCsharpFunction("{}".format(getImplName(fn_name)), fn_ret, fn_args)
  funcRet = "" if fn_ret == "void" else "return "
  return "private {}\n{{\n    {}{}!.OriginalFunction({});\n}}".format(funcSig, funcRet, getHookName(fn_name), ", ".join("{}".format(key) for key in fn_args.keys()))

def getHookName(fn_name):
  return "_{}Hook".format(fn_name)

def getReloadedHook(fn_name):
  return "private IHook<{}>? {};".format(fn_name, getHookName(fn_name))

def getScanHook(fn_name, fn_pattern):
  return "ScanHooks.Add(nameof({}), \"{}\", (hooks, result) => {} = hooks.CreateHook<{}>({}, result).Activate());".format(fn_name, fn_pattern, getHookName(fn_name), fn_name, getImplName(fn_name))

def getSHFunc(fn_name, fn_pattern):
  return "{} = new SHFunction<{}>({}, \"{}\");".format(getHookName(fn_name), fn_name, getImplName(fn_name), fn_pattern)

def process(min_length = 1):
  fm = currentProgram.getFunctionManager()
  fn = fm.getFunctionContaining(currentAddress)
  cm = currentProgram.getCodeManager()

  pattern = getPattern(fn, cm, MAKE_SIG_AT['fn'], min_length)
  fn_name = fn.getName()
  fn_ret = getCsharpType(fn.getReturnType().getName())
  fn_args = getCsharpArgs(fn)
  print(getReloadedDelegate(fn_name, fn_ret, fn_args))
  print(getReloadedHook(fn_name))
  print(getReloadedImpl(fn_name, fn_ret, fn_args))
  print(getSHFunc(fn_name, pattern))

if __name__ == "__main__":
  fm = currentProgram.getFunctionManager()
  fn = fm.getFunctionContaining(currentAddress)
  if not fn:
    printerr("Not in a function")
  else:
    process(min_length = 1)
